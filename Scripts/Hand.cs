using Godot;
using System;

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

	int handSize = 3;

	const float MaxHandWidthRatio = 0.5f;
	const int MaxHandSize = 100;

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

		float handEnclosedAngle = MathF.Min(maxHandEnclosedAngle, (handSize - 1) * defaultCardAngle);
		float handStartAngle = -handEnclosedAngle / 2 - 90;
		float cardAngle = 0;
		if (handSize > 1) cardAngle = handEnclosedAngle / (handSize - 1);

		GD.Print("Hand enclosed angle: ", handEnclosedAngle, " start angle: ", handStartAngle, " angle step: ", cardAngle);

		Vector2 handOrigin = new Vector2(
			GetViewport().GetVisibleRect().Size.X / 2, 
			GetViewport().GetVisibleRect().Size.Y - handHeight + handRadius
			);

		for (int i = 0; i < handSize; i++)
		{
			float angle = handStartAngle + cardAngle * i;
			Vector2 cardPosition = GetPointOnCircle(handOrigin, handRadius, angle);

			var card = cardScene.Instantiate<Node2D>();

			card.Position = cardPosition;
			card.Rotation = DegreeToRadian(angle + 90);

			AddChild(card);
		}
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (handSize < MaxHandSize) handSize++;
			PopulateHand();
		}

		if (@event is InputEventMouseButton mouseButton2 && mouseButton2.Pressed && mouseButton2.ButtonIndex == MouseButton.Right)
		{
			if (handSize > 1) handSize--;
			PopulateHand();
		}
    }
}
