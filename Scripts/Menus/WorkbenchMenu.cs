using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Menus;

public partial class WorkbenchMenu : Control {
  private enum CardOrigin {
    None,
    Stash,
    Slot1,
    Slot2,
    Slot3,
  }

  private enum DraggedCardState {
    None,
    OverStashArea,
    OverSlot1,
    OverSlot2,
    OverSlot3
  }

  [Export] public Player Player = null!;
  [Export] public Container StashContainer = null!;
  [Export] public Container Slot1Container = null!;
  [Export] public Container Slot2Container = null!;
  [Export] public Container Slot3Container = null!;
  [Export] public ColorRect StashArea = null!;
  [Export] public ColorRect Slot1Area = null!;
  [Export] public ColorRect Slot2Area = null!;
  [Export] public ColorRect Slot3Area = null!;
  [Export] public Label StashSizeLabel = null!;
  [Export] public Label SlotSizeLabel = null!;
  [Export] public Button CancelButton = null!;
  [Export] public Button UpgradeButton = null!;

  [Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

  private Card? _slot1;
  private Card? _slot2;
  private Card? _slot3;

  private DraggedCardState _draggedCardState;

  private const int CardsPerRow = 2;
  private CardOrigin _draggedCardOrigin;

  public override void _Ready() {
    Visible = false;
    UpgradeButton.Pressed += UpgradeButtonPressed;
    CancelButton.Pressed += Close;
  }

  public override void _Process(double delta) {
  }

  public override void _Input(InputEvent @event) {
    if (!Visible) return;
  }

  public void Open() {
    Visible = true;
    Data.MenuOpen = true;
    UpgradeButton.Disabled = true;
    Player.PutCardsInUseIntoDeck();
    FillContainersWithCardViews();
    UpdateLabels();
  }

  public void Close() {
    Player.Hand.DrawUntilFull();
    Visible = false;
    Data.MenuOpen = false;
    EmptySlots();
  }

  private void UpgradeButtonPressed() {
    if (!CardsAreValid) return;

    if (!_slot1!.Upgrade()) {
      Utils.SpawnFloatingLabel(Slot2Area, Slot2Area.Size / 2 - new Vector2(0, 100), "Already at maximum level!",
        color: Global.Red, height: 160);
      return;
    }

    Data.Stash.Add(_slot1);
    EmptySlots(false);
    FillContainersWithCardViews();
  }

  private void EmptySlots(bool returnToStash = true) {
    if (_slot1 != null) {
      if (returnToStash) Data.Stash.Add(_slot1);
      _slot1 = null;
    }

    if (_slot2 != null) {
      if (returnToStash) Data.Stash.Add(_slot2);
      _slot2 = null;
    }

    if (_slot3 != null) {
      if (returnToStash) Data.Stash.Add(_slot3);
      _slot3 = null;
    }

    UpdateUpgradeButton();
  }

  private void FillContainersWithCardViews() {
    FillScrollContainerWithCardViews(StashContainer, Data.Stash.GetCards());
    FillSlotWithCardView(Slot1Container, _slot1);
    FillSlotWithCardView(Slot2Container, _slot2);
    FillSlotWithCardView(Slot3Container, _slot3);
  }

  private void FillSlotWithCardView(Container slotContainer, Card? card) {
    foreach (var child in slotContainer.GetChildren()) child?.QueueFree();
    if (card == null) return;
    var cardContainer = new Container();
    cardContainer.SetCustomMinimumSize(Global.GlobalCardSize);
    var view = _cardScene.Instantiate<CardView>();
    view.Card = card;
    view.HoverAnimation = CardView.HoverAnimationType.Grow;
    view.Position = Global.GlobalCardSize / 2;
    view.OnDragStartEvent += OnCardDragStartEventHandler;
    view.OnDragEndEvent += OnCardDragEndEventHandler;
    view.OnDragEvent += OnCardDragEventHandler;
    cardContainer.AddChild(view);
    slotContainer.AddChild(cardContainer);
  }

  private void FillScrollContainerWithCardViews(Container container, List<Card> cards) {
    foreach (var child in container.GetChildren()) child?.QueueFree();
    HBoxContainer row = null!;
    for (var i = 0; i < cards.Count; i++) {
      var rowNumber = i / CardsPerRow;
      var card = cards[i];

      if (i % CardsPerRow == 0) {
        row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 18);
      }

      var cardContainer = new Container();
      cardContainer.SetCustomMinimumSize(Global.GlobalCardSize);
      var view = _cardScene.Instantiate<CardView>();
      view.Card = card;
      view.HoverAnimation = CardView.HoverAnimationType.Grow;
      view.Position = Global.GlobalCardSize / 2;
      view.OnDragStartEvent += OnCardDragStartEventHandler;
      view.OnDragEndEvent += OnCardDragEndEventHandler;
      view.OnDragEvent += OnCardDragEventHandler;
      cardContainer.AddChild(view);
      row.AddChild(cardContainer);

      if (i != cards.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
      row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
      container.AddChild(row);
      row = new HBoxContainer();
    }
  }

  private int GetOccupiedSlotsCount() => (_slot1 == null ? 0 : 1) + (_slot2 == null ? 0 : 1) + (_slot3 == null ? 0 : 1);

  private void UpdateLabels() {
    StashSizeLabel.Text = $"({Data.Stash.Size})";
    SlotSizeLabel.Text = $"({GetOccupiedSlotsCount()} / 3)";
  }

  private void OnCardDragStartEventHandler(CardView view) {
    _draggedCardOrigin = CardOrigin.None;
    if (_slot1 == view.Card) {
      _draggedCardOrigin = CardOrigin.Slot1;
    }
    else if (_slot2 == view.Card) {
      _draggedCardOrigin = CardOrigin.Slot2;
    }
    else if (_slot3 == view.Card) {
      _draggedCardOrigin = CardOrigin.Slot3;
    }
    else if (Data.Stash.Contains(view.Card)) {
      _draggedCardOrigin = CardOrigin.Stash;
    }
  }

  private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverSlot1Area = Slot1Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot2Area = Slot2Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot3Area = Slot3Area.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverSlot1Area && _draggedCardOrigin != CardOrigin.Slot1 && _slot1 == null) {
      if (_draggedCardState != DraggedCardState.OverSlot1) {
        _draggedCardState = DraggedCardState.OverSlot1;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverSlot2Area && _draggedCardOrigin != CardOrigin.Slot2 && _slot2 == null) {
      if (_draggedCardState != DraggedCardState.OverSlot2) {
        _draggedCardState = DraggedCardState.OverSlot2;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverSlot3Area && _draggedCardOrigin != CardOrigin.Slot3 && _slot3 == null) {
      if (_draggedCardState != DraggedCardState.OverSlot3) {
        _draggedCardState = DraggedCardState.OverSlot3;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverStashArea && _draggedCardOrigin != CardOrigin.Stash) {
      if (_draggedCardState != DraggedCardState.OverStashArea) {
        _draggedCardState = DraggedCardState.OverStashArea;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else {
      if (_draggedCardState != DraggedCardState.None) {
        _draggedCardState = DraggedCardState.None;
        view.PlayResetAnimation();
      }
    }
  }

  private void RemoveDraggedCardFromItsOrigin(Card card) {
    switch (_draggedCardOrigin) {
      case CardOrigin.Slot1:
        _slot1 = null;
        break;
      case CardOrigin.Slot2:
        _slot2 = null;
        break;
      case CardOrigin.Slot3:
        _slot3 = null;
        break;
      case CardOrigin.Stash:
        Data.Stash.Remove(card);
        break;
      case CardOrigin.None:
      default:
        break;
    }
  }

  private void OnCardDragEndEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverSlot1Area = Slot1Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot2Area = Slot2Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot3Area = Slot3Area.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverSlot1Area && _draggedCardOrigin != CardOrigin.Slot1 && _slot1 == null) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      _slot1 = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverSlot2Area && _draggedCardOrigin != CardOrigin.Slot2 && _slot2 == null) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      _slot2 = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverSlot3Area && _draggedCardOrigin != CardOrigin.Slot3 && _slot3 == null) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      _slot3 = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverStashArea && _draggedCardOrigin != CardOrigin.Stash) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      Data.Stash.Add(view.Card);
      FillContainersWithCardViews();
    }

    _draggedCardOrigin = CardOrigin.None;

    UpdateLabels();
    UpdateUpgradeButton();
  }

  private void UpdateUpgradeButton() {
    UpgradeButton.Disabled = !CardsAreValid;
  }

  private bool CardsAreValid =>
    _slot1 != null &&
    _slot2 != null &&
    _slot3 != null &&
    _slot1.GetType() == _slot2.GetType() &&
    _slot2.GetType() == _slot3.GetType() &&
    _slot1.Level == _slot2.Level &&
    _slot2.Level == _slot3.Level;
}