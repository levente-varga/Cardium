using System;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity
{
	[Export] public World World = null!;
	[Export] public Label DebugLabel = null!;
	[Export] public Hand Hand = null!;
	public readonly Deck Deck = new();
	private Pile _discardPile = new();
	
	public delegate void OnActionDelegate();
	public event OnActionDelegate? OnActionEvent;
	
	public override void _Ready()
	{
		base._Ready();
		
		BaseVision = 3;
		BaseRange = 2;
		Name = "Player";
		BaseDamage = 1;
		MaxHealth = 10;
		Health = MaxHealth;
		
		Position = Vector2I.One;

		HealthBar.Visible = true;

		FillDeck();
		Hand.Deck = Deck;
		Hand.DrawCards(Hand.Capacity);
		
		SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
		
		SetupActionListeners();
	}

	public override void _Process(double delta)
	{
		DebugLabel.Visible = Global.Debug;
		DebugLabel.Text = $"Health: {Health} / {MaxHealth}\n"
		                  + $"Position: {Position}\n"
		                  + $"Range: {Range}\n"
		                  + $"Vision: {Vision}\n"
		                  + $"Damage: {Damage}\n"
		                  + $"Armor: {Armor}\n"
		                  + $"\n"
		                  + $"Deck: {Deck.Size}/{Deck.Capacity}"
		                  + $"Hand: {Hand.Size}/{Hand.Capacity}";
		
		base._Process(delta);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!@event.IsPressed()) return;
		if (Hand.IsPlayingACard) return;
		
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

	private void FillDeck() {
		var r = new Random();
		for (var i = 0; i < Deck.Capacity; i++) {
			var type = r.Next(8);
			Card card = type switch {
				0 => new HealCard(),
				1 => new SmiteCard(),
				2 => new HurlCard(),
				3 => new PushCard(),
				4 => new ChainCard(),
				5 => new GoldenKeyCard(),
				6 => new WoodenKeyCard(),
				7 => new HolyCard(),
				_ => new ShuffleCard(),
			};
			Deck.Add(card);
		}
		Deck.Shuffle();
	}

	private void SetupActionListeners()
	{
		OnMoveEvent += OnMoveEventHandler;
		OnNudgeEvent += OnNudgeEventHandler;
		Hand.OnCardPlayedEvent += OnCardPlayedEventHandler;
	}
	
	private void OnMoveEventHandler(Vector2I from, Vector2I to) => OnActionEvent?.Invoke();
	private void OnNudgeEventHandler(Vector2I at) => OnActionEvent?.Invoke();
	private void OnCardPlayedEventHandler(Card card) {
		if (Hand.IsNotFull) Hand.DrawCards(1);
		OnActionEvent?.Invoke();
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
		Deck.Add(card);
		SpawnFloatingLabel($"x1 {card.DisplayName} card", color: new Color("F4B41B"));
		if (Hand.IsNotFull) Hand.DrawCards(1);
	}
}