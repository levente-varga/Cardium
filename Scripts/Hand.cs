using Godot;
using System;
using System.Collections.Generic;

public partial class Hand : Node2D
{
	[Export]
	public float handRadius = 1000f;

	[Export]
	public float handHeight = 64;
	
	[Export]
	public float maxHandEnclosedAngle = 30f;

	[Export]
	public float defaultCardAngle = 7f;

	List<Node2D> cards = new List<Node2D>();
	List<float> cardAngles = new List<float>();
	const int MaxHandSize = 100;
	PackedScene cardScene = null;
	Vector2 handOrigin = Vector2.Zero;


	public override void _Ready()
	{
		cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		handOrigin = new Vector2(
			GetViewport().GetVisibleRect().Size.X / 2,
			GetViewport().GetVisibleRect().Size.Y - handHeight + handRadius
			);
		
		PositionHand();
	}


	private void PositionHand()
	{	
		var oldCardAngles = new List<float>(cardAngles);
		cardAngles = GetCardAngles();

		for (int i = 0; i < cards.Count; i++)
		{
			var tween = CreateTween();
			int index = i;
			tween.TweenMethod(Callable.From<float>((value) => SetCardPosition(index, value)), oldCardAngles[index], cardAngles[index], 0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			tween.TweenCallback(Callable.From(() => { GD.Print("Tween of card #", index, " out of ",cards.Count , " complete"); }));
		}
	}


	private void SetCardPosition(int index, float angle) {
		if (index >= cards.Count || index < 0) return;
		Vector2 cardPosition = GetPointOnCircle(handOrigin, handRadius, angle);

		cards[index].Position = cardPosition;
		cards[index].Rotation = DegreeToRadian(angle + 90);
		cardAngles[index] = angle;
	}


	private List<float> GetCardAngles() => GetCardAngles(cards.Count);
	private List<float> GetCardAngles(int cardCount)
	{
		List<float> angles = new List<float>();

		float handEnclosedAngle = MathF.Min(maxHandEnclosedAngle, (cardCount - 1) * defaultCardAngle);
		float handStartAngle = -handEnclosedAngle / 2 - 90;
		float cardAngle = 0;
		if (cardCount > 1) cardAngle = handEnclosedAngle / (cardCount - 1);

		for (int i = 0; i < cardCount; i++)
		{
			float angle = handStartAngle + cardAngle * i;
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
		if (cards.Count >= MaxHandSize) return;
		var card = cardScene.Instantiate<Node2D>();
		cardAngles.Add(-45);
		cards.Add(card);
		AddChild(card);
	}


	private void RemoveCard() => RemoveCard(cards.Count - 1);
	private void RemoveCard(int i)
	{
		if (cards.Count == 0) return;
		cardAngles.RemoveAt(i);
		cards[i].QueueFree();
		cards.RemoveAt(i);
	}


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			AddCard();
			PositionHand();
		}

		if (@event is InputEventMouseButton mouseButton2 && mouseButton2.Pressed && mouseButton2.ButtonIndex == MouseButton.Right)
		{
			RemoveCard();
			PositionHand();
		}
    }
}
