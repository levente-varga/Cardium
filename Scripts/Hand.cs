using Godot;
using System;

public partial class Hand : Node2D
{
	[Export]
	public Curve cardHorizontalPositions;

	[Export]
	public Curve cardVerticalOffsets;

	[Export]
	public Curve cardRotations;

	int handSize = 3;

	const float MaxHandWidthRatio = 0.5f;
	const int MaxHandSize = 10;
	const int HandHeight = 160;

	public override void _Ready()
	{
		PopulateHand();
	}

	private void PopulateHand()
	{		
		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}

		var cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");


		float weightStart = (1f - (float)(handSize - 1) / (MaxHandSize - 1)) / 2f;

		GD.Print("Weight start: ", weightStart);

		for (int i = 0; i < handSize; i++)
		{
			float weight = 0.5f;
			if (handSize > 1) {
				weight = weightStart + ((float)i / (MaxHandSize - 1));
			}

			var card = cardScene.Instantiate<Container>();

			var positionSample = cardHorizontalPositions.Sample(weight);
			var offsetSample = cardVerticalOffsets.Sample(weight);
			var rotationSample = cardRotations.Sample(weight);

			card.Position = new Vector2(
				DisplayServer.WindowGetSize().X / 2 + positionSample * MaxHandWidthRatio * DisplayServer.WindowGetSize().X / 2f, 
				DisplayServer.WindowGetSize().Y - HandHeight + offsetSample * 100
				);

			card.Rotation = rotationSample * 0.3f;

			AddChild(card);

			GD.Print("Added a card at: ", card.Position, " with rotation: ", card.Rotation, " (weight: ", weight, ")");
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
