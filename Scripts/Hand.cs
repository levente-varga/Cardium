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
	[Export] public float TiltAngle = 0f;
	[Export] public Vector2 Origin = Vector2.Zero;
	
	public bool RightHanded = true;
	public HandState State { get; private set; }

	private readonly List<Card> _cards = new();
	private List<float> _cardAngles = new();

	private int _capacity = 5;
	public int Capacity {
		get => _capacity;
		private set => _capacity = Math.Max(value, 1);
	}
	public int Size => _cards.Count;
	private Rect2 _playArea;
	
	public delegate void OnCardPlayedDelegate(Card card);
	public event OnCardPlayedDelegate? OnCardPlayedEvent;

	public override void _Ready()
	{
		Origin = new Vector2(
			0,
			GetViewport().GetVisibleRect().Size.Y - HandHeight + HandRadius / 2
		);
		
		PositionHand();
		
		_playArea = new Rect2(
			0,
			0,
			GetViewport().GetVisibleRect().Size.X,
			GetViewport().GetVisibleRect().Size.Y / (3f / 2f)
		);
	}
	
	public override void _Process(double delta)
	{
		
	}


	private void PositionHand()
	{	
		var oldCardAngles = new List<float>(_cardAngles);
		_cardAngles = GetCardAngles();

		for (var i = 0; i < _cards.Count; i++)
		{
			var tween = CreateTween();
			var index = i;
			tween.TweenMethod(Callable.From<float>(value => SetCardPosition(index, value)), oldCardAngles[index], _cardAngles[index], 0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			tween.TweenCallback(Callable.From(() => { tween.Dispose(); }));
		}
	}


	private void SetCardPosition(int index, float angle) {
		if (index >= _cards.Count || index < 0) return;
		Vector2 cardPosition = GetPointOnCircle(Origin, HandRadius, angle);

		_cards[index].Position = cardPosition;
		_cards[index].Rotation = DegreeToRadian(angle + 90);
		//_cards[index].ZIndex = index;
		_cardAngles[index] = angle;
	}


	private List<float> GetCardAngles() => GetCardAngles(_cards.Count);
	private List<float> GetCardAngles(int cardCount)
	{
		var angles = new List<float>();

		var handEnclosedAngle = MathF.Min(MaxHandEnclosedAngle, (cardCount - 1) * DefaultCardAngle);
		var handStartAngle = 270 - handEnclosedAngle / 2;
		float cardAngle = 0;
		if (cardCount > 1) cardAngle = handEnclosedAngle / (cardCount - 1);

		for (var i = 0; i < cardCount; i++)
		{
			var angle = handStartAngle + cardAngle * i;
			angles.Add(angle);
		}

		return angles;
	}


	private float DegreeToRadian(float angle)
	{
		return (float)(MathF.PI * angle / 180.0);
	}


	private float RadianToDegree(float angle)
	{
		return (float)(angle * (180.0 / MathF.PI));
	}


	private Vector2 GetPointOnCircle(float radius, float angle)
	{
		return new Vector2(
			radius * MathF.Cos(DegreeToRadian(angle)),
			radius * MathF.Sin(DegreeToRadian(angle))
		);
	}


	private Vector2 GetPointOnCircle(Vector2 origin, float radius, float angle)
	{
		return origin + GetPointOnCircle(radius, angle);
	}
	
	
	public void Add(Card card) => Add(card, _cards.Count);
	public void Add(Card card, int index)
	{
		if (_cards.Count >= Capacity) return;
		_cardAngles.Add(315);
		_cards.Insert(index, card);
		AddChild(card);
		PositionHand();
		
		card.OnDragEndEvent += OnCardDragEnd;
		card.OnDragEvent += OnCardDrag;
	}

	private void RemoveLast() => Remove(_cards.Last());
	private void Remove(int index) => Remove(_cards[index]);
	private void Remove(Card card)
	{
		if (!_cards.Contains(card)) return;
		_cardAngles.RemoveAt(_cards.IndexOf(card));
		card.OnDragEndEvent -= OnCardDragEnd;
		card.OnDragEvent -= OnCardDrag;
		_cards.Remove(card);
		card.QueueFree();
		PositionHand();
	}


	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventKey { Pressed: true, KeyLabel: Key.Key1 }:
				Add(new HealCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key2 }:
				Add(new SmiteCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key3 }:
				Add(new HurlCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key4 }:
				Add(new PushCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key5 }: 
				Add(new ChainCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key6 }:
				Add(new KeyCard());
				break;
		}
	}

	private void OnCardDrag(Card card, Vector2 mousePosition)
	{
		State = HandState.Dragging;
		switch (card.InPlayArea)
		{
			case false when _playArea.HasPoint(mousePosition):
				card.OnEnterPlayArea();
				Utils.SpawnFallingLabel(GetTree(), card.GlobalPosition, "Entered play area");
				break;
			case true when !_playArea.HasPoint(mousePosition):
				card.OnExitPlayArea();
				Utils.SpawnFallingLabel(GetTree(), card.GlobalPosition, "Exited play area");
				break;
		}
	}
	
	private void OnCardDragEnd(Card card, Vector2 mousePosition)
	{
		if (card.InPlayArea) card.OnExitPlayArea();

		State = HandState.Idle;
		
		if (!_playArea.HasPoint(mousePosition)) return;
		
		State = HandState.Playing;
		
		card.OnEnterPlayArea();
		_ = Play(card);
	}

	private async Task Play(Card card)
	{
		//Enabled = false;
		var success = await Player.World.PlayCard(card);
		//Enabled = true;
		
		if (success)
		{
			Remove(card);
			PositionHand();
		}
		else card.OnExitPlayArea();

		if (success)
		{
			OnCardPlayedEvent?.Invoke(card);
		}
		else
		{
			Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Cancelled");
		}
		
		State = HandState.Idle;
	}
	
	private void EnableCards(bool enable)
	{
		foreach (var card in _cards)
		{
			card.Enabled = enable;
		}
		
		Utils.SpawnFallingLabel(GetTree(), Player.GlobalPosition, "Hand " + (enable ? "enabled" : "disabled"));
	}
}