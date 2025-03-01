using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity
{
	[Export] public World World;
	
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();
	private Hand _hand = new();
	
	private bool _nextToObject = false;
	
	public override void _Ready()
	{
		base._Ready();
		
		Vision = 3.5f;
		Name = "Player";
		Damage = 1;

		HealthBar.Visible = false;

		SetTexture(GD.Load<Texture2D>("res://assets/player.png"));
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
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
			case InputEventKey { Pressed: true, Keycode: Key.Q }:
				if (!World.EnemyExistsAt(Position + Vector2I.Up))
				{
					GD.Print("No enemy to attack at " + (Position + Vector2I.Up));
					break;
				}
				Enemy enemy = World.GetEnemyAt(Position + Vector2I.Up);
				if (enemy == null)
				{
					GD.Print("Null returned as enemy at " + (Position + Vector2I.Up));
					break;
				}
				GD.Print("Attacking " + enemy.Name);
				
				Attack(enemy);
				break;
		}
	}
	
	public void CheckForObjects()
	{
		_nextToObject = World.ObjectExistsAt(Position + Vector2I.Up);
	}
	
	public void Attack(Entity target)
	{
		World.Attack(target, this);
	}
}