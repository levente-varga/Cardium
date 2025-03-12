using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts;

public partial class Hand : Node2D
{
	[Export] public Player Player;
	[Export] public float HandRadius = 1000f;
	[Export] public float HandHeight = 64;
	[Export] public float MaxHandEnclosedAngle = 30f;
	[Export] public float DefaultCardAngle = 7f;
	[Export] public float TiltAngle = 0f;
	[Export] public Vector2 Origin = Vector2.Zero;
	
	public bool RightHanded = true;

	private readonly List<Card> _cards = new();
	private List<float> _cardAngles = new();
	private const int MaxHandSize = 10;
	private Rect2 _playArea;

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
			GetViewport().GetVisibleRect().Size.Y -400
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


	public void AddCard()
	{
		if (_cards.Count >= MaxHandSize) return;
		var card = new HealCard();
		_cardAngles.Add(315);
		_cards.Add(card);
		AddChild(card);
		PositionHand();
		
		card.OnDragEndEvent += OnCardDragEnd;
		card.OnDragEvent += OnCardDrag;
	}


	private void RemoveCard() => RemoveCard(_cards.Count - 1);
	private void RemoveCard(int i)
	{
		if (_cards.Count == 0) return;
		_cardAngles.RemoveAt(i);
		_cards[i].QueueFree();
		_cards.RemoveAt(i);
		PositionHand();
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, KeyLabel: Key.P })
		{
			AddCard();
		}
		if (@event is InputEventKey { Pressed: true, KeyLabel: Key.O })
		{
			RemoveCard();
		}
	}

	private void OnCardDrag(Card card, Vector2 mousePosition)
	{
		if (_playArea.HasPoint(mousePosition)) card.OnEnterPlayArea();
		else card.OnExitPlayArea();
	}
	
	private void OnCardDragEnd(Card card, Vector2 mousePosition)
	{
		if (!_playArea.HasPoint(mousePosition)) return;
		RemoveCard(_cards.IndexOf(card));
		PositionHand();
		card.OnPlay(Player);
	}
}