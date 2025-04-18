using System;
using System.Linq;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity {
  [Export] public World World = null!;
  [Export] public Label DebugLabel = null!;
  [Export] public Hand Hand = null!;

  public delegate void OnActionDelegate();

  public event OnActionDelegate? OnActionEvent;

  public override void _Ready() {
    base._Ready();

    BaseVision = 3;
    BaseRange = 2;
    Name = "Player";
    BaseDamage = 1;
    MaxHealth = 10;
    Health = MaxHealth;

    Position = Vector2I.One;

    HealthBar.Visible = true;

    Hand.Deck.FillWithRandom();
    Hand.DrawCards(Hand.Capacity);

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Player.png"), 8, 12);

    SetupActionListeners();
  }

  public override void _Process(double delta) {
    DebugLabel.Visible = Global.Debug;
    DebugLabel.Text = $"Health: {Health} / {MaxHealth}\n"
                      + $"Position: {Position}\n"
                      + $"Range: {Range}\n"
                      + $"Vision: {Vision}\n"
                      + $"Damage: {Damage}\n"
                      + $"Armor: {Armor}\n"
                      + $"\n"
                      + $"Deck: {Hand.Deck.Deck.Size}/{Hand.Deck.Deck.Capacity}"
                      + $"Hand: {Hand.Size}/{Hand.Capacity}";

    base._Process(delta);
  }

  public override void _Input(InputEvent @event) {
    if (!@event.IsPressed()) return;
    if (Hand.IsPlayingACard) return;

    if (InputMap.EventIsAction(@event, "Interact")) {
    }
    else if (InputMap.EventIsAction(@event, "Use")) {
    }
    else if (InputMap.EventIsAction(@event, "Reset")) {
      GetTree().ReloadCurrentScene();
    }
    else if (InputMap.EventIsAction(@event, "Back")) {
      GetTree().Quit();
    }

    if (InputMap.EventIsAction(@event, "Right")) {
      Move(Direction.Right, World);
    }
    else if (InputMap.EventIsAction(@event, "Left")) {
      Move(Direction.Left, World);
    }
    else if (InputMap.EventIsAction(@event, "Up")) {
      Move(Direction.Up, World);
    }
    else if (InputMap.EventIsAction(@event, "Down")) {
      Move(Direction.Down, World);
    }
    else if (InputMap.EventIsAction(@event, "Skip")) {
    }
  }

  private void SetupActionListeners() {
    OnMoveEvent += OnMoveEventHandler;
    OnNudgeEvent += OnNudgeEventHandler;
    Hand.OnCardPlayedEvent += OnCardPlayedEventHandler;
    Hand.OnCardDiscardedEvent += OnCardDiscardedEventHandler;
  }

  private void OnMoveEventHandler(Vector2I from, Vector2I to) => OnActionEvent?.Invoke();
  private void OnNudgeEventHandler(Vector2I at) => OnActionEvent?.Invoke();
  private void OnCardPlayedEventHandler(Card card) => OnActionEvent?.Invoke();
  private void OnCardDiscardedEventHandler(Card card) => OnActionEvent?.Invoke();

  protected override void Nudge(Direction direction) {
    var i = World.GetInteractableAt(Position + DirectionToVector(direction));
    if (i is Bonfire) {
      i.OnInteract(this, World.Camera);
    }
    else {
      // TODO: Could implement door and chest nudge
    }

    base.Nudge(direction);
  }

  public void Interact() {
    var interactablePositions = World.GetInteractablePositions(Position);
    if (interactablePositions.Count == 0) return;

    World.Interact(interactablePositions[0]);
  }

  protected override void TakeTurn(Player player, World world) {
    if (Global.Debug) SpawnDebugFloatingLabel("Start of turn");
  }

  public void PickUpCard(Card card) {
    Hand.Deck.Add(card);
    SpawnFloatingLabel($"x1 {card.Name} card", color: new Color("F4B41B"));
    if (Hand.IsNotFull) Hand.DrawCards(1);
  }
}