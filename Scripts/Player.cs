using Godot;

namespace Cardium.Scripts;

public partial class Player : Sprite2D
{
	[Export] public World World;
	
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
	
	private Vector2I _position = Vector2I.Zero;
	
	
	public override void _Ready()
	{
		// Set the player's position to the center of the screen
		Position = _position * 64;

		// Set the player's texture
		Texture = GD.Load<Texture2D>("res://assets/player.png");
	}

	public override void _Process(double delta)
	{
		// Update the player's position
		Position = _position * 64;
	}
	
	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventKey { Pressed: true, Keycode: Key.Right }:
				if (World.IsTileEmpty(_position + new Vector2I(1, 0)))
					_position.X++;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Left }:
				if (World.IsTileEmpty(_position + new Vector2I(-1, 0)))
					_position.X--;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Up }:
				if (World.IsTileEmpty(_position + new Vector2I(0, -1)))
					_position.Y--;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Down }:
				if (World.IsTileEmpty(_position + new Vector2I(0, 1)))
					_position.Y++;
				break;
		}
	}
}