using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity {
  [Export] public World World = null!;
  [Export] public Label DebugLabel = null!;
  [Export] public Hand Hand = null!;
  [Export] public PileView DiscardPile = null!;
  [Export] public DeckView Deck = null!;

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
    MaxHealth = 10;
    Health = MaxHealth;
    StatusBar.Reset();

    Position = Vector2I.One;

    StatusBar.Visible = Data.ShowHealth;

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

    Statistics.Steps++;
    Move(_moveDirection!.Value, World);
    _lastMoveMsec = Time.GetTicksMsec();
    _moveDelayMsec = MinMoveDelayMsec + (BaseMoveDelayMsec - MinMoveDelayMsec) / ++_consecutiveMoves;
  }

  public void ReloadDeck(List<Type> except) {
    var cards = new List<Card>(DiscardPile.Pile.Cards);
    foreach (var card in cards) {
      if (except.Contains(card.GetType())) continue;
      if (!Deck.Add(card)) break;
      DiscardPile.Remove(card);
    }

    Hand.DrawUntilFull();
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
    Statistics.Nudges++;
    
    var i = World.GetInteractableAt(Position + DirectionToVector(direction));
    i?.OnNudge(this, World);

    base.Nudge(direction);
  }

  public void Interact() {
    var interactablePositions = World.GetInteractablePositions(Position);
    if (interactablePositions.Count == 0) return;

    World.Interact(interactablePositions[0]);
  }

  protected override void OnDamaged(Entity source, int damage, World world) {
    base.OnDamaged(source, damage, world);
    
    Statistics.TotalDamageTaken += damage;
  }

  protected override void OnHealed(int amount) {
    base.OnHealed(amount);
    
    Statistics.TotalHealAmount += amount;
  }

  public void PutCardsInUseIntoDeck() {
    var cards = new List<Card>(Hand.Cards);
    foreach (var card in cards) {
      Deck.Add(card);
      Hand.Remove(card);
    }

    cards = new List<Card>(DiscardPile.Pile.Cards);
    foreach (var card in cards) {
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
      card.Origin = Card.Origins.Inventory;
      Inventory.Add(card);
      Statistics.CardsCollected++;
      SpawnFloatingLabel($"x1 {card.Name} (lvl. {card.Level}){(card.Protected ? " [protected]" : "")}", card.RarityColor, 120 + i * 40, fontSize: 28);
    }
  }

  public void SaveCards() {
    Data.Deck.Clear();
    Data.Inventory.Clear();
    Data.Deck.AddAll(Deck.Deck.Cards);
    Data.Deck.AddAll(DiscardPile.Pile.Cards);
    Data.Deck.AddAll(Hand.Cards);
    Data.Inventory.AddAll(Inventory.Cards);
  }

  public void LoadCards() {
    Deck.Clear();
    Inventory.Clear();
    foreach (var card in Data.Deck.Cards) Deck.Add(card);
    foreach (var card in Data.Inventory.Cards) Inventory.Add(card);
  }
}