using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Menus;

public partial class WorkbenchMenu : Menu {
  private enum DraggedCardOrigin {
    None,
    List,
    Slot0,
    Slot1,
    Slot2,
    Result,
  }

  private enum DraggedCardState {
    None,
    OverListArea,
    OverSlot1,
    OverSlot2,
    OverSlot3,
  }

  [Export] public Player Player = null!;
  [Export] public Container ListContainer = null!;
  [Export] public Container Slot1Container = null!;
  [Export] public Container Slot2Container = null!;
  [Export] public Container Slot3Container = null!;
  [Export] public Container ResultContainer = null!;
  [Export] public ColorRect ListArea = null!;
  [Export] public ColorRect Slot1Area = null!;
  [Export] public ColorRect Slot2Area = null!;
  [Export] public ColorRect Slot3Area = null!;
  [Export] public Label ListSizeLabel = null!;
  [Export] public Label SlotSizeLabel = null!;
  [Export] public Button CancelButton = null!;

  [Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");
  
  private readonly List<Card?> _slots = new(3) { null, null, null };

  private Card? _result;

  private DraggedCardState _draggedCardState;

  private const int CardsPerRow = 2;
  private DraggedCardOrigin _draggedCardOrigin;

  public override void _Ready() {
    Visible = false;
    CancelButton.Pressed += Close;
  }
  public override void Open() {
    base.Open();
    Player.PutCardsInUseIntoDeck();
    Player.SaveCards();
    FillContainersWithCardViews();
    UpdateLabels();
  }

  public override void Close() {
    base.Close();
    EmptySlots();
    Player.LoadCards();
    Player.Hand.DrawUntilFull();
  }

  private void EmptySlots(bool returnToList = true) {
    for (var i = 0; i < _slots.Count; i++) EmptySlot(i, returnToList);
    _result = null;
  }

  private void EmptySlot(int index, bool returnToList = true) {
    if (_slots[index] != null) {
      if (returnToList) ReturnCardToItsOrigin(_slots[index]!);
      _slots[index] = null;
    }
  }

  private void FillContainersWithCardViews() {
    FillScrollContainerWithCardViews();
    FillSlotWithCardView(Slot1Container, _slots[0]);
    FillSlotWithCardView(Slot2Container, _slots[1]);
    FillSlotWithCardView(Slot3Container, _slots[2]);
    FillSlotWithCardView(ResultContainer, _result);
  }

  private void FillSlotWithCardView(Container slotContainer, Card? card) {
    foreach (var child in slotContainer.GetChildren()) if (child is Container) child.QueueFree();
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

  private void FillScrollContainerWithCardViews() {
    foreach (var child in ListContainer.GetChildren()) child?.QueueFree();

    var cards = new List<Card>();
    var views = new List<CardView>();

    cards.AddRange(Data.Stash.Cards);
    cards.AddRange(Data.Inventory.Cards);
    cards.AddRange(Data.Deck.Cards);
    
    Utils.SortCards(cards);
    
    for (var i = 0; i < cards.Count; i++) {
      var view = _cardScene.Instantiate<CardView>();
      view.Card = cards[i];
      view.HoverAnimation = CardView.HoverAnimationType.Grow;
      view.Position = Global.GlobalCardSize / 2;
      view.OnDragStartEvent += OnCardDragStartEventHandler;
      view.OnDragEndEvent += OnCardDragEndEventHandler;
      view.OnDragEvent += OnCardDragEventHandler;
      views.Add(view);
    }
    
    HBoxContainer row = null!;
    for (var i = 0; i < views.Count; i++) {
      var rowNumber = i / CardsPerRow;

      if (i % CardsPerRow == 0) {
        row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 18);
      }

      var cardContainer = new Container();
      cardContainer.SetCustomMinimumSize(Global.GlobalCardSize);
      cardContainer.AddChild(views[i]);
      row.AddChild(cardContainer);

      if (i != cards.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
      row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
      ListContainer.AddChild(row);
      row = new HBoxContainer();
    }
  }

  private int OccupiedSlotsCount => _slots.Sum(slot => slot != null ? 1 : 0);

  private void UpdateLabels() {
    ListSizeLabel.Text = $"({Data.TotalCardCount})";
    SlotSizeLabel.Text = $"({OccupiedSlotsCount} / 3)";
  }

  private void OnCardDragStartEventHandler(CardView view) {
    _draggedCardOrigin = DraggedCardOrigin.None;
    if (_slots[0] == view.Card) {
      _draggedCardOrigin = DraggedCardOrigin.Slot0;
    }
    else if (_slots[1] == view.Card) {
      _draggedCardOrigin = DraggedCardOrigin.Slot1;
    }
    else if (_slots[2] == view.Card) {
      _draggedCardOrigin = DraggedCardOrigin.Slot2;
    }
    else if (_result == view.Card) {
      _draggedCardOrigin = DraggedCardOrigin.Result;
    }
    else if (Data.GetAllCards().Contains(view.Card)) {
      _draggedCardOrigin = DraggedCardOrigin.List;
    }
  }

  private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverSlot1Area = Slot1Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot2Area = Slot2Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot3Area = Slot3Area.GetRect().HasPoint(mousePosition);
    var mouseOverListArea = ListArea.GetRect().HasPoint(mousePosition);

    if (mouseOverSlot1Area && _draggedCardOrigin != DraggedCardOrigin.Slot0 && _slots[0] == null) {
      if (_draggedCardState != DraggedCardState.OverSlot1) {
        _draggedCardState = DraggedCardState.OverSlot1;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverSlot2Area && _draggedCardOrigin != DraggedCardOrigin.Slot1 && _slots[1] == null) {
      if (_draggedCardState != DraggedCardState.OverSlot2) {
        _draggedCardState = DraggedCardState.OverSlot2;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverSlot3Area && _draggedCardOrigin != DraggedCardOrigin.Slot2 && _slots[2] == null) {
      if (_draggedCardState != DraggedCardState.OverSlot3) {
        _draggedCardState = DraggedCardState.OverSlot3;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverListArea && _draggedCardOrigin != DraggedCardOrigin.List) {
      if (_draggedCardState != DraggedCardState.OverListArea) {
        _draggedCardState = DraggedCardState.OverListArea;
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

  private void RemoveDraggedCardFromItsOrigin(CardView view) {
    switch (_draggedCardOrigin) {
      case DraggedCardOrigin.Slot0:
        _slots[0] = null;
        break;
      case DraggedCardOrigin.Slot1:
        _slots[1] = null;
        break;
      case DraggedCardOrigin.Slot2:
        _slots[2] = null;
        break;
      case DraggedCardOrigin.Result:
        _result = null;
        break;
      case DraggedCardOrigin.List:
        switch (view.Card.Origin) {
          case Card.Origins.Deck:
            Data.Deck.Remove(view.Card);
            break;
          case Card.Origins.Inventory:
            Data.Inventory.Remove(view.Card);
            break;
          case Card.Origins.Stash:
            Data.Stash.Remove(view.Card);
            break;
          case Card.Origins.None:
          default: 
            break;
        }
        break;
      case DraggedCardOrigin.None:
      default:
        break;
    }
  }
  
  private void ReturnCardToItsOrigin(Card card) {
    switch (card.Origin) {
      case Card.Origins.Deck:
        Data.Deck.Add(card);
        break;
      case Card.Origins.Inventory:
        Data.Inventory.Add(card);
        break;
      case Card.Origins.Stash:
        Data.Stash.Add(card);
        break;
      case Card.Origins.None:
      default: 
        break;
    }
  }

  private void OnCardDragEndEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverSlot1Area = Slot1Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot2Area = Slot2Area.GetRect().HasPoint(mousePosition);
    var mouseOverSlot3Area = Slot3Area.GetRect().HasPoint(mousePosition);
    var mouseOverListArea = ListArea.GetRect().HasPoint(mousePosition);

    if (mouseOverSlot1Area && _draggedCardOrigin != DraggedCardOrigin.Slot0 && (_slots[0] == null || view.Card == _result)) {
      if (view.Card == _result) {
        Statistics.CardsUpgraded++;
        EmptySlots(false);
      }
      RemoveDraggedCardFromItsOrigin(view);
      _slots[0] = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverSlot2Area && _draggedCardOrigin != DraggedCardOrigin.Slot1 && (_slots[1] == null || view.Card == _result)) {
      if (view.Card == _result) {
        Statistics.CardsUpgraded++;
        EmptySlots(false);
      }
      RemoveDraggedCardFromItsOrigin(view);
      _slots[1] = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverSlot3Area && _draggedCardOrigin != DraggedCardOrigin.Slot2 && (_slots[2] == null || view.Card == _result)) {
      if (view.Card == _result) {
        Statistics.CardsUpgraded++;
        EmptySlots(false);
      }
      RemoveDraggedCardFromItsOrigin(view);
      _slots[2] = view.Card;
      FillContainersWithCardViews();
    }
    else if (mouseOverListArea && _draggedCardOrigin != DraggedCardOrigin.List) {
      RemoveDraggedCardFromItsOrigin(view);
      switch (view.Card.Origin) {
        case Card.Origins.Deck:
          Data.Deck.Add(view.Card);
          break;
        case Card.Origins.Inventory:
          Data.Inventory.Add(view.Card);
          break;
        case Card.Origins.Stash:
        case Card.Origins.None:
        default:
          Data.Stash.Add(view.Card);
          break;
      }
      if (_draggedCardOrigin == DraggedCardOrigin.Result) {
        Statistics.CardsUpgraded++;
        EmptySlots(false);
      }
      FillContainersWithCardViews();
    }

    if (_result == null && CardsAreValid) {
      var isProtected = _slots[0]!.Protected || _slots[1]!.Protected || _slots[2]!.Protected;

      _result = CreateUpgradedCard(_slots[0]!.GetType(), _slots[0]!.Level + 1);
      
      if (_result == null) {
        Utils.SpawnFloatingLabel(Slot2Area, Slot2Area.Size / 2 - new Vector2(0, 100), "Already at maximum level!",
          color: Global.Red, height: 160);
        return;
      }
      else {
        _result.Origin = Card.Origins.Stash;
        _result.Protected = isProtected;
      }
      FillContainersWithCardViews();
    }
    else if (!CardsAreValid && _result != null) {
      _result = null;
      FillContainersWithCardViews();
    }
    

    _draggedCardOrigin = DraggedCardOrigin.None;

    UpdateLabels();
  }

  private bool CardsAreValid => _slots.All(slot => slot != null) &&
    _slots[0]!.GetType() == _slots[1]!.GetType() &&
    _slots[1]!.GetType() == _slots[2]!.GetType() &&
    _slots[0]!.Level == _slots[1]!.Level &&
    _slots[1]!.Level == _slots[2]!.Level;
  
  private Card? CreateUpgradedCard(Type cardType, int level) {
    GD.Print($"Creating upgraded card variant...");

    if (!typeof(Card).IsAssignableFrom(cardType)) {
      GD.Print($"Can't cast {cardType} to Card!");
      return null;
    }

    var card = (Card)Activator.CreateInstance(cardType)!;

    int upgradedLevel;
    for (upgradedLevel = 0; upgradedLevel < level; upgradedLevel++) {
      if (!card.Upgrade()) break;
    }

    if (upgradedLevel != level) {
      GD.Print($"Can't upgrade");
    }
    else {
      GD.Print($"Upgraded!");
    }
    
    return upgradedLevel == level ? card : null;
  }
}