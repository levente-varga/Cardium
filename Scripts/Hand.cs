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

	int handSize = 0;

	const float handWidthRatio = 0.5f;
	const int handHeight = 100;

	public override void _Ready()
	{
		PopulateHand(10);
	}

	private void PopulateHand(int size)
	{		
		var cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

		for (int i = 0; i < size; i++)
		{
			float weight = (float)i / (size - 1);
			var card = cardScene.Instantiate<Node2D>();

			var positionSample = cardHorizontalPositions.Sample(weight);
			var offsetSample = cardVerticalOffsets.Sample(weight);
			var rotationSample = cardRotations.Sample(weight);

			card.Position = new Vector2(
				positionSample * (DisplayServer.WindowGetSize().X * handWidthRatio) + DisplayServer.WindowGetSize().X * handWidthRatio / 2, 
				DisplayServer.WindowGetSize().Y - handHeight + offsetSample * 100
				);

			card.Rotate(rotationSample * 0.3f);

			AddChild(card);

			GD.Print("Added a card at: ", card.Position, " with rotation: ", card.Rotation, " (weight: ", weight, ")");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
