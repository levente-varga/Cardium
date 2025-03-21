using System.Threading.Tasks;
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
	
	private bool _nextToObject = false;
	private bool _turnOngoing = false;

	public bool CanMove => Hand.State == Hand.HandState.Idle && (!InCombat || InCombat && _turnOngoing);
	
	public override void _Ready()
	{
		base._Ready();
		
		BaseVision = 4;
		BaseRange = 2;
		Name = "Player";
		BaseDamage = 1;
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
			OnTurnEnd();
			//return;
		}

		DebugLabel.Visible = Global.Debug;
		DebugLabel.Text = $"Health: {Health} / {MaxHealth}\n"
		                  + $"Energy: {Energy} / {MaxEnergy}\n"
		                  + $"Position: {Position}\n"
		                  + $"InCombat: {InCombat}\n"
		                  + $"Turn: {_turnOngoing}\n"
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
		
		if (!CanMove) return;
		
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

	public void Interact()
	{
		var interactablePositions = World.GetInteractablePositions(Position);
		if (interactablePositions.Count == 0) return;
		
		World.Interact(interactablePositions[0]);
	}

	protected override async Task Turn(Player player, World world)
	{
		if (Global.Debug) SpawnDebugFloatingLabel("Start of turn");
		_turnOngoing = true;

		await WhileTurnOngoing();
	}
	
	private async Task WhileTurnOngoing()
	{
		while (_turnOngoing)
		{ 
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}
	
	public void EnableHand(bool enable)
	{
		Hand.Enabled = enable;
	}
	
	public void PickUpCard(Card card)
	{
		Hand.AddCard(card);
	}
}