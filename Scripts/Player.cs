using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

public partial class Player : Entity {
  [Export] public World World = null!;
  [Export] public Label DebugLabel = null!;
  [Export] public Hand Hand = null!;

  public delegate void OnActionDelegate();

  public event OnActionDelegate? OnActionEvent;

  private Direction? _moveDirection;
  private ulong? _lastMoveMsec;
  private const ulong BaseMoveDelayMsec = 375;
  private const ulong MinMoveDelayMsec = 125;
  private ulong _moveDelayMsec = BaseMoveDelayMsec;
  private ulong _consecutiveMoves = 0;

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

    HandleMovement();
    
    base._Process(delta);
  }

  private void HandleMovement() {
    if (Hand.IsPlayingACard) return;

    var lastMoveDirection = _moveDirection;
    _moveDirection = null;
    if (Input.IsActionPressed("Right")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Right && Input.IsActionJustPressed("Right")) {
        _moveDirection = Direction.Right;
      }
    }
    if (Input.IsActionPressed("Left")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Left && Input.IsActionJustPressed("Left")) {
        _moveDirection = Direction.Left;
      }
    }
    if (Input.IsActionPressed("Up")) {
      if (_moveDirection == null || lastMoveDirection != Direction.Up && Input.IsActionJustPressed("Up")) {
        _moveDirection = Direction.Up;
      }
    }
    if (Input.IsActionPressed("Down")) {
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
    if (!@event.IsPressed()) return;
    
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
    Inventory.Add(card);
    SpawnFloatingLabel($"x1 {card.Name} card", card.RarityColor);
  }
}