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
	const int MaxHandSize = 100;
	PackedScene cardScene = null;


	public override void _Ready()
	{
		cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
		PositionHand();
	}


	private void PositionHand()
	{		
		List<float> cardAngles = GetCardAngles();

		Vector2 handOrigin = new Vector2(
			GetViewport().GetVisibleRect().Size.X / 2, 
			GetViewport().GetVisibleRect().Size.Y - handHeight + handRadius
			);

		for (int i = 0; i < cards.Count; i++)
		{
			float angle = cardAngles[i];
			Vector2 cardPosition = GetPointOnCircle(handOrigin, handRadius, angle);

			cards[i].Position = cardPosition;
			cards[i].Rotation = DegreeToRadian(angle + 90);
		}
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
		cards.Add(card);
		AddChild(card);
	}


	private void RemoveCard() => RemoveCard(cards.Count - 1);
	private void RemoveCard(int i)
	{
		if (cards.Count == 0) return;
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
