using Godot;
using System;

public partial class Hand : Node2D
{
	[Export]
	public float handRadius = 100f;

	[Export]
	public float handHeight = 200f;
	
	[Export]
	public float handMaxEnclosedAngle = 30f;

	[Export]
	public float cardAngle = 5f;

	int handSize = 3;

	const float MaxHandWidthRatio = 0.5f;
	const int MaxHandSize = 10;

	public override void _Ready()
	{
		PopulateHand();
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

	private void PopulateHand()
	{		
		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}

		var cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		float handEnclosedAngle = (handSize - 1) * cardAngle;
		float handStartAngle = -MathF.Min(handMaxEnclosedAngle, handEnclosedAngle) / 2 - 90;
		float angleStep = 0;
		if (handSize > 1) angleStep = handEnclosedAngle / (handSize - 1);

		Vector2 handOrigin = new Vector2(
			DisplayServer.WindowGetSize().X / 2, 
			DisplayServer.WindowGetSize().Y - handHeight + handRadius
			);

		for (int i = 0; i < handSize; i++)
		{
			float angle = handStartAngle + angleStep * i;
			Vector2 cardPosition = GetPointOnCircle(handOrigin, handRadius, angle);

			var card = cardScene.Instantiate<Node2D>();

			card.Position = cardPosition;
			card.Rotation = DegreeToRadian(angle + 90);

			AddChild(card);

			GD.Print("Added a card at: ", card.Position, " with rotation: ", card.Rotation);
		}
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (handSize < 10) handSize++;
			PopulateHand();
		}

		if (@event is InputEventMouseButton mouseButton2 && mouseButton2.Pressed && mouseButton2.ButtonIndex == MouseButton.Right)
		{
			if (handSize > 1) handSize--;
			PopulateHand();
		}
    }
}
