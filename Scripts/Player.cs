using Godot;

namespace Cardium.Scripts;

public partial class Player : Sprite2D
{
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();
	private Hand _hand = new();
	
	private int _maxHealth = 5;
	private int _health = 5;
	private int _maxEnergy = 5;
	private int _energy = 5;
	private int _armor = 0;
	private int _attack = 1;
	private int _luck = 0;
	
	
	public override void _Ready()
	{
		// Set the player's position to the center of the screen
		Position = new Vector2(64, 64);

		// Set the player's texture
		Texture = GD.Load<Texture2D>("res://assets/player.png");
	}

	public override void _Process(double delta)
	{
		
	}
}