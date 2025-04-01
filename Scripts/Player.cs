using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity
{
	[Export] public World World;
	[Export] public Label DebugLabel;
	[Export] public Hand Hand;
	
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();

	public int BaseVision;
	public int TempVision;
	public int Vision => BaseVision + TempVision;
	
	public override void _Ready()
	{
		base._Ready();
		
		BaseVision = 4;
		BaseRange = 2;
		Name = "Player";
		BaseDamage = 1;
		MaxHealth = 5;
		Health = MaxHealth;

		HealthBar.Visible = false;
		
		SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
	}

	public override void _Process(double delta)
	{
		DebugLabel.Visible = Global.Debug;
		DebugLabel.Text = $"Health: {Health} / {MaxHealth}\n"
		                  + $"Position: {Position}\n"
		                  + $"Range: {Range}\n"
		                  + $"Vision: {Vision}\n"
		                  + $"Damage: {Damage}\n"
		                  + $"Armor: {Armor}\n";
		
		base._Process(delta);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!@event.IsPressed()) return;
		
		if (InputMap.EventIsAction(@event, "Interact"))
		{
			
		}
		else if (InputMap.EventIsAction(@event, "Use"))
		{
			
		}
		else if (InputMap.EventIsAction(@event, "Reset"))
		{
			GetTree().ReloadCurrentScene();
		}
		else if (InputMap.EventIsAction(@event, "Back"))
		{
			GetTree().Quit();
		}
		
		if (InputMap.EventIsAction(@event, "Right"))
		{
			Move(Direction.Right, World);
		}
		else if (InputMap.EventIsAction(@event, "Left"))
		{
			Move(Direction.Left, World);
		}
		else if (InputMap.EventIsAction(@event, "Up"))
		{
			Move(Direction.Up, World);
		}
		else if (InputMap.EventIsAction(@event, "Down"))
		{
			Move(Direction.Down, World);
		}
		else if (InputMap.EventIsAction(@event, "Skip"))
		{
			
		}
	}

	public void Interact()
	{
		var interactablePositions = World.GetInteractablePositions(Position);
		if (interactablePositions.Count == 0) return;
		
		World.Interact(interactablePositions[0]);
	}

	protected override void TakeTurn(Player player, World world)
	{
		if (Global.Debug) SpawnDebugFloatingLabel("Start of turn");
	}
	
	public void PickUpCard(Card card)
	{
		Hand.AddCard(card);
	}
}