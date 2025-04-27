using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity {
  [Export] public World World = null!;
  [Export] public Label DebugLabel = null!;
  [Export] public Hand Hand = null!;
  [Export] public PileView DiscardPile = null!;
  [Export] public DeckView Deck = null!;
  [Export] public InventoryMenu InventoryMenu = null!;
  [Export] public WorkbenchMenu WorkbenchMenu = null!;

  public delegate void OnActionDelegate();

  public event OnActionDelegate? OnActionEvent;

  private Direction? _moveDirection;
  private ulong? _lastMoveMsec;
  private const ulong BaseMoveDelayMsec = 375;
  private const ulong MinMoveDelayMsec = 125;
  private ulong _moveDelayMsec = BaseMoveDelayMsec;
  private ulong _consecutiveMoves;

  private readonly HashSet<string> _blockedActions = new() { "Right", "Left", "Up", "Down" };

  public override void _Ready() {
    base._Ready();

    foreach (var action in _blockedActions) {
      if (!Input.IsActionPressed(action))
        _blockedActions.Remove(action);
    }

    BaseVision = 3;
    BaseRange = 2;
    Name = "Player";
    BaseDamage = 1;
    MaxHealth = 20;
    Health = MaxHealth;

    Position = Vector2I.One;

    HealthBar.Visible = Data.ShowHealth;

    LoadCards();
    Hand.DrawUntilFull();

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
                      + $"Deck: {Deck.Deck.Size}/{Deck.Deck.Capacity}"
                      + $"Hand: {Hand.Size}/{Hand.Capacity}";

    HandleMovement();

    base._Process(delta);
  }

  private bool IsActionAllowed(string action) {
    if (_blockedActions.Count == 0 || !_blockedActions.Contains(action)) {
      return Input.IsActionPressed(action);
    }

    if (!Input.IsActionPressed(action))
      _blockedActions.Remove(action);
    return false;
  }

  private void HandleMovement() {
    if (Data.MenuOpen || Hand.IsPlayingACard) return;

    var lastMoveDirection = _moveDirection;
    _moveDirection = null;
    if (IsActionAllowed("Right")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Right && Input.IsActionJustPressed("Right")) {
        _moveDirection = Direction.Right;
      }
    }

    if (IsActionAllowed("Left")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Left && Input.IsActionJustPressed("Left")) {
        _moveDirection = Direction.Left;
      }
    }

    if (IsActionAllowed("Up")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Up && Input.IsActionJustPressed("Up")) {
        _moveDirection = Direction.Up;
      }
    }

    if (IsActionAllowed("Down")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Down && Input.IsActionJustPressed("Down")) {
        _moveDirection = Direction.Down;
      }
    }

    if (_moveDirection != null && _moveDirection != lastMoveDirection) {
      _lastMoveMsec = null;
    }

    var timeSinceLastMove = _lastMoveMsec == null ? ulong.MaxValue : Time.GetTicksMsec() - _lastMoveMsec;
    if (timeSinceLastMove < _moveDelayMsec) return;

    if (_moveDirection == null) {
      _lastMoveMsec = null;
      _consecutiveMoves = 0;
      _moveDelayMsec = BaseMoveDelayMsec;
      return;
    }

    Move(_moveDirection!.Value, World);
    _lastMoveMsec = Time.GetTicksMsec();
    _moveDelayMsec = MinMoveDelayMsec + (BaseMoveDelayMsec - MinMoveDelayMsec) / ++_consecutiveMoves;
  }

  public override void _Input(InputEvent @event) {
    if (Data.MenuOpen || Hand.IsPlayingACard) return;

    if (!@event.IsPressed()) return;

    if (InputMap.EventIsAction(@event, "Interact")) {
    }
    else if (InputMap.EventIsAction(@event, "Use")) {
    }
    else if (InputMap.EventIsAction(@event, "Reset")) {
      GetTree().ReloadCurrentScene();
    }
    else if (InputMap.EventIsAction(@event, "Skip")) {
    }
    else if (InputMap.EventIsAction(@event, "Reload")) {
      ReloadDeck();
    }
  }

  private void ReloadDeck() {
    var cards = DiscardPile.Pile.GetCards();
    foreach (var card in cards) {
      if (Deck.Deck.IsFull) return;
      DiscardPile.Remove(card);
      Deck.Add(card);
    }

    Hand.DrawUntilFull();
    TakeTurn();
  }

  private void SetupActionListeners() {
    OnMoveEvent += OnMoveEventHandler;
    OnNudgeEvent += OnNudgeEventHandler;
    Hand.OnCardPlayedEvent += OnCardPlayedEventHandler;
    Hand.OnCardDiscardedEvent += OnCardDiscardedEventHandler;
  }

  private void OnMoveEventHandler(Vector2I from, Vector2I to) => TakeTurn();
  private void OnNudgeEventHandler(Vector2I at) => TakeTurn();
  private void OnCardPlayedEventHandler(Card card) => TakeTurn();
  private void OnCardDiscardedEventHandler(Card card) => TakeTurn();

  private void TakeTurn() {
    TurnsLived++;
    OnActionEvent?.Invoke();
  }

  protected override void Nudge(Direction direction) {
    var i = World.GetInteractableAt(Position + DirectionToVector(direction));
    i?.OnNudge(this, World.Camera);

    base.Nudge(direction);
  }

  public void Interact() {
    var interactablePositions = World.GetInteractablePositions(Position);
    if (interactablePositions.Count == 0) return;

    World.Interact(interactablePositions[0]);
  }

  public void PutCardsInUseIntoDeck() {
    foreach (var card in Hand.GetCards()) {
      Deck.Add(card);
      Hand.Remove(card);
    }

    foreach (var card in DiscardPile.GetCards()) {
      Deck.Add(card);
      DiscardPile.Remove(card);
    }
  }

  protected override void TakeTurn(Player player, World world) {
    if (Global.Debug) SpawnDebugFloatingLabel("Start of turn");
  }

  public void PickUpCard(Card card) => PickUpCards(new List<Card> { card });

  public void PickUpCards(List<Card> cards) {
    for (var i = 0; i < cards.Count; i++) {
      var card = cards[i];
      Inventory.Add(card);
      SpawnFloatingLabel($"x1 {card.Name} (lvl. {card.Level})", card.RarityColor, 120 + i * 40, fontSize: 28);
    }
  }

  public void SaveCards() {
    Data.Deck.Clear();
    Data.Inventory.Clear();
    foreach (var card in Deck.Deck.GetCards()) Data.Deck.Add(card);
    foreach (var card in DiscardPile.GetCards()) Data.Deck.Add(card);
    foreach (var card in Hand.GetCards()) Data.Deck.Add(card);
    foreach (var card in Inventory.GetCards()) Data.Inventory.Add(card);
  }

  public void LoadCards() {
    foreach (var card in Data.Deck.GetCards()) Deck.Add(card);
    foreach (var card in Data.Inventory.GetCards()) Inventory.Add(card);
  }
}