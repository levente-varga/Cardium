using Godot;

namespace Cardium.Scripts;

public partial class Player : Sprite2D
{
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set the player's position to the center of the screen
		Position = GetViewportRect().Size / 8;

		// Set the player's texture
		Texture = GD.Load<Texture2D>("res://assets/player.png");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}