using Godot;
using System;
using System.Drawing;

public partial class Card : Node2D
{
	AnimationPlayer animationPlayer = null;
	Node2D body = null;
	Sprite2D sprite = null;
	Control hitbox = null;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		body = GetNode<Node2D>("Body");
		sprite = GetNode<Sprite2D>("Body/Sprite");
		hitbox = GetNode<Control>("Body/Hitbox");

		SetupHitbox();
	}

	private void SetupHitbox()
	{
		var cardSize = sprite.Texture.GetSize() * sprite.Scale;
		hitbox.Size = cardSize;
		hitbox.Position = new Vector2(-cardSize.X / 2, -cardSize.Y / 2);

		hitbox.MouseEntered += OnMouseEntered;
		hitbox.MouseExited += OnMouseExited;
	}

	public override void _Process(double delta)
	{
	}


	public void OnMouseEntered() {
		animationPlayer.Play("Select");
	}

	public void OnMouseExited() {
		animationPlayer.Play("Deselect");
	}
}
