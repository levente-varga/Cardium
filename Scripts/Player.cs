using Godot;

namespace Cardium.Scripts;

public partial class Player : Sprite2D
{
	[Export] public World World;
	
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();
	private Hand _hand = new();
	
	public int MaxHealth => 5;
	public int Health { get; private set; } = 5;
	public int MaxEnergy => 5;
	public int Energy { get; private set; } = 5;
	public int Armor => 0;
	public int Damage => 1;
	public int Luck => 0;
	public float Vision => 3.5f;
	public new Vector2I Position { get; private set; }
	
	public override void _Ready()
	{
		// Set the player's position to the center of the screen
		Position = Vector2I.Zero;

		// Set the player's texture
		Texture = GD.Load<Texture2D>("res://assets/player.png");
	}

	public override void _Process(double delta)
	{
		base.Position = base.Position.Lerp(Position * 64, 0.1f);
	}
	
	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventKey { Pressed: true, Keycode: Key.Right }:
				if (World.IsTileEmpty(Position + Vector2I.Right))
					Position += Vector2I.Right;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Left }:
				if (World.IsTileEmpty(Position + Vector2I.Left))
					Position += Vector2I.Left;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Up }:
				if (World.IsTileEmpty(Position + Vector2I.Up))
					Position += Vector2I.Up;
				break;
			case InputEventKey { Pressed: true, Keycode: Key.Down }:
				if (World.IsTileEmpty(Position + Vector2I.Down))
					Position += Vector2I.Down;
				break;
		}
	}
}