using Godot;
using System;

public partial class Card : Node2D
{
	AnimationPlayer animationPlayer = null;
	Sprite2D cardSprite = null;

	Vector2 cardSize;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		cardSprite = GetNode<Sprite2D>("CardSprite");

		cardSize = cardSprite.Texture.GetSize() * cardSprite.Scale;

		GD.Print("Card ready");
	}

	public override void _Process(double delta)
	{
	}


	public void OnMouseEntered() {
		GD.Print("Mouse entered");
		animationPlayer.Play("Select");
	}

	public void OnMouseExited() {
		GD.Print("Mouse exited");
		animationPlayer.Play("Deselect");
	}
}
