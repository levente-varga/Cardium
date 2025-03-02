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

		SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!@event.IsPressed()) return;
		
		if (InputMap.EventIsAction(@event, "Right"))
		{
			if (World.IsTileEmpty(Position + Vector2I.Right)) Position += Vector2I.Right;
			else Nudge(Vector2I.Right);
		}
		else if (InputMap.EventIsAction(@event, "Left"))
		{
			if (World.IsTileEmpty(Position + Vector2I.Left)) Position += Vector2I.Left;
			else Nudge(Vector2I.Left);
		}
		else if (InputMap.EventIsAction(@event, "Up"))
		{
			if (World.IsTileEmpty(Position + Vector2I.Up)) Position += Vector2I.Up;
			else Nudge(Vector2I.Up);
		}
		else if (InputMap.EventIsAction(@event, "Down"))
		{
			if (World.IsTileEmpty(Position + Vector2I.Down)) Position += Vector2I.Down;
			else Nudge(Vector2I.Down);
		}
		else if (InputMap.EventIsAction(@event, "Interact"))
		{
			Interact();
		}
		else if (InputMap.EventIsAction(@event, "Use"))
		{
			Attack();
		}
		else if (Input.IsKeyPressed(Key.R))
		{
			GetTree().ReloadCurrentScene();
		}
	}
	
	public void CheckForObjects()
	{
		_nextToObject = World.InteractableExistsAt(Position + Vector2I.Up);
	}
	
	public void Attack()
	{
		World.Attack(Position + Vector2I.Up, this);
	}

	public void Interact()
	{
		var interactablePositions = World.GetInteractablePositions(Position);
		if (interactablePositions.Count == 0) return;
		
		World.Interact(interactablePositions[0]);
	}
}