using Godot;
using System;
using System.Collections.Generic;

public partial class Hand : Node2D
{
	[Export]
	public float HandRadius = 1000f;

	[Export]
	public float HandHeight = 64;
	
	[Export]
	public float MaxHandEnclosedAngle = 30f;

	[Export]
	public float DefaultCardAngle = 7f;

	private readonly List<Node2D> _cards = new();
	private List<float> _cardAngles = new();
	private const int MaxHandSize = 10;
	private PackedScene _cardScene;
	private Vector2 _handOrigin = Vector2.Zero;


	public override void _Ready()
	{
		_cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		_handOrigin = new Vector2(
			GetViewport().GetVisibleRect().Size.X / 2,
			GetViewport().GetVisibleRect().Size.Y - HandHeight + HandRadius
			);
		
		PositionHand();
	}


	private void PositionHand()
	{	
		var oldCardAngles = new List<float>(_cardAngles);
		_cardAngles = GetCardAngles();

		for (var i = 0; i < _cards.Count; i++)
		{
			var tween = CreateTween();
			var index = i;
			tween.TweenMethod(Callable.From<float>((value) => SetCardPosition(index, value)), oldCardAngles[index], _cardAngles[index], 0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			tween.TweenCallback(Callable.From(() => { GD.Print("Tween of card #", index, " out of ",_cards.Count , " complete"); }));
		}
	}


	private void SetCardPosition(int index, float angle) {
		if (index >= _cards.Count || index < 0) return;
		Vector2 cardPosition = GetPointOnCircle(_handOrigin, HandRadius, angle);

		_cards[index].Position = cardPosition;
		_cards[index].Rotation = DegreeToRadian(angle + 90);
		_cardAngles[index] = angle;
	}


	private List<float> GetCardAngles() => GetCardAngles(_cards.Count);
	private List<float> GetCardAngles(int cardCount)
	{
		var angles = new List<float>();

		var handEnclosedAngle = MathF.Min(MaxHandEnclosedAngle, (cardCount - 1) * DefaultCardAngle);
		var handStartAngle = -handEnclosedAngle / 2 - 90;
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


	private void AddCard()
	{
		if (_cards.Count >= MaxHandSize) return;
		var card = _cardScene.Instantiate<Node2D>();
		_cardAngles.Add(-45);
		_cards.Add(card);
		AddChild(card);
	}


	private void RemoveCard() => RemoveCard(_cards.Count - 1);
	private void RemoveCard(int i)
	{
		if (_cards.Count == 0) return;
		_cardAngles.RemoveAt(i);
		_cards[i].QueueFree();
		_cards.RemoveAt(i);
	}


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
		{
			AddCard();
			PositionHand();
		}

		if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right }) return;
		RemoveCard();
		PositionHand();
    }
}
