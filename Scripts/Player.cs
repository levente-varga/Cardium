using System.Threading.Tasks;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity
{
	[Export] public World World;
	[Export] public Label DebugLabel;
	
	private Deck _combatDeck = new();
	private Deck _actionDeck = new();
	private Pile _discardPile = new();
	private Hand _hand = new();
	
	private bool _nextToObject = false;
	private bool _turnOngoing = false;
	
	public override void _Ready()
	{
		base._Ready();
		
		Vision = 3.5f;
		Name = "Player";
		Damage = 1;
		MaxHealth = 5;
		Health = MaxHealth;
		MaxEnergy = 3;
		Energy = MaxEnergy;

		HealthBar.Visible = false;
		EnergyBar.Visible = false;
		
		SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
	}

	public override void _Process(double delta)
	{
		if (Energy <= 0 && _turnOngoing)
		{
			_turnOngoing = false;
			OnTurnFinished();
			//return;
		}

		DebugLabel.Text = "";
		if (Global.Debug) DebugLabel.Text = $"Health: {Health} / {MaxHealth}\n"
		                                    + $"Energy: {Energy} / {MaxEnergy}\n"
		                                    + $"Position: {Position}\n"
		                                    + $"InCombat: {InCombat}\n"
		                                    + $"Turn: {_turnOngoing}\n";
		
		base._Process(delta);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!@event.IsPressed()) return;
		
		if (InputMap.EventIsAction(@event, "Interact"))
		{
			Interact();
		}
		else if (InputMap.EventIsAction(@event, "Use"))
		{
			Attack();
		}
		else if (InputMap.EventIsAction(@event, "Reset"))
		{
			GetTree().ReloadCurrentScene();
		}
		else if (InputMap.EventIsAction(@event, "Back"))
		{
			GetTree().Quit();
		}
		
		if (InCombat && !_turnOngoing) return;
		
		if (InputMap.EventIsAction(@event, "Right"))
		{
			Move(Direction.Right, World, useEnergy: InCombat);
		}
		else if (InputMap.EventIsAction(@event, "Left"))
		{
			Move(Direction.Left, World, useEnergy: InCombat);
		}
		else if (InputMap.EventIsAction(@event, "Up"))
		{
			Move(Direction.Up, World, useEnergy: InCombat);
		}
		else if (InputMap.EventIsAction(@event, "Down"))
		{
			Move(Direction.Down, World, useEnergy: InCombat);
		}
		else if (InputMap.EventIsAction(@event, "Skip"))
		{
			Energy--;
		}
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

	public override async Task OnTurn(Player player, World world)
	{
		TurnMarker.Visible = true;
		
		if (Global.Debug) SpawnFloatingLabel("[Debug] Start of turn", color: Global.Magenta, fontSize: 20);
		Energy = MaxEnergy;
		_turnOngoing = true;
	}
}