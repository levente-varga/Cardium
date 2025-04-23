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
	public enum HandState { None, Dragging, Playing }
	public enum CardInHandState { None, OverPlayArea, OverDiscardArea }
	
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
	[Export] public DeckView Deck = null!;
	
	[Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

	private CardView? _hovered;
	
	public HandState State { get; private set; }

	private readonly List<CardView> _cardViews = new();
	private Dictionary<CardView, float> _cardAngles = new();
	private readonly Dictionary<CardView, CardInHandState> _cardStates = new();

	private int _capacity = 4;
	public int Capacity {
		get => _capacity;
		private set => _capacity = Math.Max(value, 1);
	}
	
	public int Size => _cardViews.Count;
	public bool IsFull => _cardViews.Count == Capacity;
	public bool IsNotFull => _cardViews.Count < Capacity;
	public bool IsPlayingACard => State != HandState.None;
	
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

	public void DrawUntilFull() => DrawCards(Capacity - Size);
	public void DrawCards(int count, bool positionHand = true) {
		for (var i = 0; i < count; i++) {
			var card = Deck.DrawCard();
			if (card == null) break;
			Add(card, false);
			GD.Print($"Hand size: {Size}");
		}
		if (positionHand) PositionCards();
	}

	private void PositionCards() {
		GD.Print($"Positioning hand ({Size})");
		var oldCardAngles = new Dictionary<CardView, float>(_cardAngles);
		_cardAngles = GetCardAngles();

		for (var i = 0; i < _cardViews.Count; i++) {
			var tween = CreateTween();
			var index = i;
			var view = _cardViews[i];
			
			tween.TweenMethod(Callable.From<float>(value => SetCardPosition(index, value)), 
					oldCardAngles[view],
					_cardAngles[view], 
					0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo)
				.SetDelay(0.08 * index);
			tween.TweenCallback(Callable.From(() => { tween.Dispose(); }));
		}
	}
	
	private void SetCardPosition(int index, float angle) {
		if (index >= Size || index < 0) return;
		var cardPosition = GetPointOnCircle(Origin, HandRadius, angle);

		var view = _cardViews[index];
		view.Position = cardPosition;
		view.RotationDegrees = angle + 90;
		_cardAngles[view] = angle;
	}


	private Dictionary<CardView, float> GetCardAngles() => GetCardAngles(Size);
	private Dictionary<CardView, float> GetCardAngles(int cardCount) {
		var angles = new Dictionary<CardView, float>();

		var handEnclosedAngle = MathF.Min(MaxHandEnclosedAngle, (cardCount - 1) * DefaultCardAngle);
		var handStartAngle = 270 - handEnclosedAngle / 2;
		float cardAngle = 0;
		if (cardCount > 1) cardAngle = handEnclosedAngle / (cardCount - 1);

		for (var i = 0; i < cardCount; i++) {
			var angle = handStartAngle + cardAngle * i;
			angles.Add(_cardViews[i], angle);
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
	
	public void Add(Card card, bool positionHand = true) => Add(card, Size, positionHand);
	public void Add(Card card, int index, bool positionHand = true) {
		if (Size >= Capacity) return;
		var view = _cardScene.Instantiate<CardView>();
		view.Card = card;
		view.OnDragEndEvent += OnCardDragEnd;
		view.OnDragEvent += OnCardDrag;
		view.OnMouseEnteredEvent += OnCardMouseEntered;
		view.OnMouseExitedEvent += OnCardMouseExited;
		_cardStates.Add(view, CardInHandState.None);
		_cardAngles.Add(view, 360);
		_cardViews.Insert(index, view);
		AddChild(view);
		SetCardPosition(index, _cardAngles[view]);
		if (positionHand) PositionCards();
	}

	public bool RemoveLast(bool positionHand = true) => Size > 0 && Remove(Size - 1, positionHand);
	public bool Remove(int index, bool positionHand = true) => Size > index && index > 0 && Remove(_cardViews[index].Card, positionHand);
	public bool Remove(Card card, bool positionHand = true) {
		var view = _cardViews.FirstOrDefault(view => view.Card == card);
		if (view == null) return false;
		
		view.OnDragEndEvent -= OnCardDragEnd;
		view.OnDragEvent -= OnCardDrag;
		view.OnMouseEnteredEvent -= OnCardMouseEntered;
		view.OnMouseExitedEvent -= OnCardMouseExited;
		_cardAngles.Remove(view);
		_cardStates.Remove(view);
		_cardViews.Remove(view);
		RemoveChild(view);
		
		if (positionHand) PositionCards();
		return true;
	}

	public List<Card> GetCards() => _cardViews.Select(view => view.Card).ToList();

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
			if (_cardStates[view] != CardInHandState.OverPlayArea) {
				_cardStates[view] = CardInHandState.OverPlayArea;
				view.PlayScaleAnimation(1.2f);
			}
		}
		else {
			if (_cardStates[view] == CardInHandState.OverPlayArea) {
				_cardStates[view] = CardInHandState.None;
				view.PlayResetAnimation();
			}
		}
		
		if (mouseOverDiscardArea) {
			if (_cardStates[view] != CardInHandState.OverDiscardArea) {
				_cardStates[view] = CardInHandState.OverDiscardArea;
				view.PlayScaleAnimation(0.75f);
			}
		}
		else {
			if (_cardStates[view] == CardInHandState.OverDiscardArea) {
				_cardStates[view] = CardInHandState.None;
				view.PlayResetAnimation();
			}
		}
	}
	
	private void OnCardDragEnd(CardView view, Vector2 mousePosition) {
		if (_cardStates[view] == CardInHandState.OverPlayArea) {
			view.OnEnterPlayingMode();
			State = HandState.Playing;
			_ = Play(view);
		}
		else {
			if (_cardStates[view] == CardInHandState.OverDiscardArea) {
				Discard(view);
				OnCardDiscardedEvent?.Invoke(view.Card);
			}
			State = HandState.None;
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
			Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition + Global.GlobalTileSize / 2, "Cancelled", Global.Red);
		}
		
		State = HandState.None;
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
	}
}