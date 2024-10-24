using Godot;
using System;

public partial class Card : Container
{
	AnimationPlayer animationPlayer = null;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;

		GD.Print("Card ready");
	}

	public override void _Process(double delta)
	{
	}

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
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
