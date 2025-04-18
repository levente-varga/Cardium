using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Hand : Node2D
{
	public enum HandState { Idle, Dragging, Playing }
	
	[Export] public Player Player = null!;
	[Export] public float HandRadius = 1000f;
	[Export] public float HandHeight = 64;
	[Export] public float MaxHandEnclosedAngle = 30f;
	[Export] public float DefaultCardAngle = 7f;
	[Export] public float TiltAngle;
	[Export] public Vector2 Origin = Vector2.Zero;

	[Export] public Control PlayArea = null!;
	[Export] public Control DiscardArea = null!;
	[Export] public PileView DiscardPile = null!;
	
	[Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

	public Deck Deck = new();
	private CardView? _hovered;
	
	public HandState State { get; private set; }

	private readonly List<Card> _cards = new();
	private readonly List<CardView> _cardViews = new();
	private List<float> _cardAngles = new();

	private int _capacity = 4;
	public int Capacity {
		get => _capacity;
		private set => _capacity = Math.Max(value, 1);
	}
	
	public int Size => _cards.Count;
	public bool IsFull => _cards.Count == Capacity;
	public bool IsNotFull => _cards.Count < Capacity;
	public bool IsPlayingACard => State != HandState.Idle;
	
	public delegate void OnCardPlayedDelegate(Card card);
	public event OnCardPlayedDelegate? OnCardPlayedEvent;
	
	public delegate void OnCardDiscardedDelegate(Card card);
	public event OnCardDiscardedDelegate? OnCardDiscardedEvent;

	public override void _EnterTree() {
		Origin = new (0, GetViewport().GetVisibleRect().Size.Y - HandHeight + HandRadius / 2);
	}

	public override void _Ready() {
		PositionCards();
	}

	public void DrawCards(int count, bool positionHand = true) {
		for (var i = 0; i < count; i++) {
			var card = Deck.Draw();
			if (card == null) break;
			Add(card, false);
			GD.Print($"Hand size: {Size}");
		}
		if (positionHand) PositionCards();
	}

	private void PositionCards() {
		GD.Print($"Positioning hand ({Size})");
		var oldCardAngles = new List<float>(_cardAngles);
		_cardAngles = GetCardAngles();

		for (var i = 0; i < _cards.Count; i++) {
			var tween = CreateTween();
			var index = i;
			
			tween.TweenMethod(Callable.From<float>(value => SetCardPosition(index, value)), 
					oldCardAngles[index],
					_cardAngles[i], 
					0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo)
				.SetDelay(0.08 * index);
			tween.TweenCallback(Callable.From(() => { tween.Dispose(); }));
		}
	}
	
	private void SetCardPosition(int index, float angle) {
		if (index >= _cards.Count || index < 0) return;
		var cardPosition = GetPointOnCircle(Origin, HandRadius, angle);

		_cardViews[index].Position = cardPosition;
		_cardViews[index].Rotation = DegreeToRadian(angle + 90);
		_cardAngles[index] = angle;
	}


	private List<float> GetCardAngles() => GetCardAngles(_cards.Count);
	private List<float> GetCardAngles(int cardCount) {
		var angles = new List<float>();

		var handEnclosedAngle = MathF.Min(MaxHandEnclosedAngle, (cardCount - 1) * DefaultCardAngle);
		var handStartAngle = 270 - handEnclosedAngle / 2;
		float cardAngle = 0;
		if (cardCount > 1) cardAngle = handEnclosedAngle / (cardCount - 1);

		for (var i = 0; i < cardCount; i++) {
			var angle = handStartAngle + cardAngle * i;
			angles.Add(angle);
		}

		return angles;
	}

	private float DegreeToRadian(float angle) => (float)(MathF.PI * angle / 180.0);
	private float RadianToDegree(float angle) => (float)(angle * (180.0 / MathF.PI));

	private Vector2 GetPointOnCircle(float radius, float angle) {
		return new Vector2(
			radius * MathF.Cos(DegreeToRadian(angle)),
			radius * MathF.Sin(DegreeToRadian(angle))
		);
	}

	private Vector2 GetPointOnCircle(Vector2 origin, float radius, float angle) {
		return origin + GetPointOnCircle(radius, angle);
	}
	
	public void Add(Card card, bool positionHand = true) => Add(card, _cards.Count, positionHand);
	public void Add(Card card, int index, bool positionHand = true) {
		if (_cards.Count >= Capacity) return;
		_cardAngles.Add(360);
		_cards.Insert(index, card);
		var view = _cardScene.Instantiate<CardView>();
		view.Init(card);
		view.OnDragEndEvent += OnCardDragEnd;
		view.OnDragEvent += OnCardDrag;
		view.OnMouseEnteredEvent += OnCardMouseEntered;
		view.OnMouseExitedEvent += OnCardMouseExited;
		_cardViews.Insert(index, view);
		AddChild(view);
		SetCardPosition(index, _cardAngles[index]);
		if (positionHand) PositionCards();
	}

	public Card? RemoveLast(bool positionHand = true) => _cards.Count > 0 ? Remove(_cards.Last(), positionHand) : null;
	public Card? Remove(int index, bool positionHand = true) => _cards.Count > index ? Remove(_cards[index], positionHand) : null;
	public Card? Remove(Card card, bool positionHand = true) {
		if (!_cards.Contains(card)) return null;
		_cardAngles.RemoveAt(_cards.IndexOf(card));
		_cards.Remove(card);
		var view = _cardViews.FirstOrDefault(view => view.Card == card);
		if (view != null) {
			view.OnDragEndEvent -= OnCardDragEnd;
			view.OnDragEvent -= OnCardDrag;
			view.OnMouseEnteredEvent -= OnCardMouseEntered;
			view.OnMouseExitedEvent -= OnCardMouseExited;
			_cardViews.Remove(view);
			RemoveChild(view);
		}
		if (positionHand) PositionCards();
		return card;
	}

	public List<Card> GetCards() => new(_cards);

	public override void _Input(InputEvent @event) {
		Card? card = @event switch {
			InputEventKey { Pressed: true, KeyLabel: Key.Key1 } => new HealCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key2 } => new SmiteCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key3 } => new HurlCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key4 } => new PushCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key5 } => new ChainCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key6 } => new GoldenKeyCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key7 } => new GoldenKeyCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key8 } => new HolyCard(),
			InputEventKey { Pressed: true, KeyLabel: Key.Key9 } => new ShuffleCard(),
			_ => null
		};

		if (card == null) return;

		RemoveLast(false);
		Add(card);
	}

	private void OnCardMouseEntered(CardView view) => _hovered = view;
	private void OnCardMouseExited(CardView view) {
		if (_hovered == view) _hovered = null;
	}

	private void OnCardDrag(CardView view, Vector2 mousePosition) {
		State = HandState.Dragging;

		var mouseOverPlayArea = PlayArea.GetRect().HasPoint(mousePosition);
		var mouseOverDiscardArea = DiscardArea.GetRect().HasPoint(mousePosition);

		if (mouseOverPlayArea) {
			if (!view.OverPlayArea) view.OnEnterPlayArea();
		}
		else {
			if (view.OverPlayArea) view.OnExitPlayArea();
		}
		
		if (mouseOverDiscardArea) {
			if (!view.OverDiscardArea) view.OnEnterDiscardArea();
		}
		else {
			if (view.OverDiscardArea) view.OnExitDiscardArea();
		}
	}
	
	private void OnCardDragEnd(CardView view, Vector2 mousePosition) {
		if (view.OverPlayArea) {
			view.OnEnterPlayingMode();
			State = HandState.Playing;
			_ = Play(view);
		}
		else {
			if (view.OverDiscardArea) {
				Discard(view);
				OnCardDiscardedEvent?.Invoke(view.Card);
			}
			State = HandState.Idle;
		}
	}

	private async Task Play(CardView view) {
		EnableCards(false);
		var success = await Player.World.PlayCard(view.Card);
		EnableCards();
		
		if (success) {
			if (view.Card.Unstable) Redraw(view);
			else Discard(view);
			OnCardPlayedEvent?.Invoke(view.Card);
		}
		else {
			view.OnExitPlayingMode();
			Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Cancelled");
		}
		
		State = HandState.Idle;
	}

	/// <summary>
	/// Removes a card from hand and draws a new card.
	/// Removed card is lost forever.
	/// </summary>
	/// <param name="view"></param>
	private void Redraw(CardView view) {
		Remove(view.Card);
		if (IsNotFull) DrawCards(1);
		else PositionCards();
	}
	
	/// <summary>
	///	Removes a card from hand and puts it onto the discard pile, then draws a new card.
	/// </summary>
	/// <param name="view"></param>
	private void Discard(CardView view) {
		Redraw(view);
		DiscardPile.Add(view.Card);
	} 
	
	private void EnableCards(bool enable = true) {
		foreach (var view in _cardViews) {
			view.Enabled = enable;
		}
		
		Utils.SpawnFallingLabel(GetTree(), Player.GlobalPosition, "Hand " + (enable ? "enabled" : "disabled"));
	}
}