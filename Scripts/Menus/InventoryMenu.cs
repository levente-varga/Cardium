using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Menus;

public partial class InventoryMenu : Menu {
  private enum CardOrigin {
    None,
    Deck,
    Stash,
    Inventory,
  }

  private enum DraggedCardState {
    None,
    OverDeckArea,
    OverInventoryArea,
    OverStashArea
  }

  [Export] public Player Player = null!;
  [Export] public VBoxContainer StashContainer = null!;
  [Export] public VBoxContainer InventoryContainer = null!;
  [Export] public VBoxContainer DeckContainer = null!;
  [Export] public ColorRect StashArea = null!;
  [Export] public ColorRect InventoryArea = null!;
  [Export] public ColorRect DeckArea = null!;
  [Export] public Label StashSizeLabel = null!;
  [Export] public Label InventorySizeLabel = null!;
  [Export] public Label DeckSizeLabel = null!;
  [Export] public Button OkButton = null!;

  [Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

  private bool _stashEnabled;

  private DraggedCardState _draggedCardState;

  private const int CardsPerRow = 2;
  private CardOrigin _draggedCardOrigin;

  public override void _Ready() {
    Visible = false;
    OkButton.Pressed += Close;
  }

  public override void _Process(double delta) {
  }

  public override void _Input(InputEvent @event) {
    if (!Visible) return;
  }

  public void Open(bool enableStash = false) {
    base.Open();
    _stashEnabled = enableStash;
    Player.PutCardsInUseIntoDeck();
    FillContainersWithCardViews();
    UpdateLabels();
    if (!_stashEnabled) StashArea.Color = new Color("16161688");
  }

  public override void Close() {
    base.Close();
    Player.Hand.DrawUntilFull();
    _stashEnabled = false;
    StashArea.Color = new Color("16161600");
  }

  private void FillContainersWithCardViews() {
    FillContainerWithCardViews(StashContainer, Data.Stash.GetCards());
    FillContainerWithCardViews(InventoryContainer, Player.Inventory.GetCards());
    FillContainerWithCardViews(DeckContainer, Player.Deck.Deck.GetCards());
  }

  private void FillContainerWithCardViews(Container container, List<Card> cards) {
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
      view.Enabled = container != StashContainer || _stashEnabled;
      view.Draggable = container != StashContainer || _stashEnabled;
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

  public void UpdateLabels() {
    StashSizeLabel.Text = $"({Data.Stash.Size})";
    InventorySizeLabel.Text = $"({Player.Inventory.Size})";
    DeckSizeLabel.Text = $"({Player.Deck.Deck.Size} / {Player.Deck.Deck.Capacity})";
  }

  private void OnCardDragStartEventHandler(CardView view) {
    _draggedCardOrigin = CardOrigin.None;
    if (Player.Inventory.Contains(view.Card)) {
      _draggedCardOrigin = CardOrigin.Inventory;
    }
    else if (Player.Deck.Deck.Contains(view.Card)) {
      _draggedCardOrigin = CardOrigin.Deck;
    }
    else if (Data.Stash.Contains(view.Card)) {
      _draggedCardOrigin = CardOrigin.Stash;
    }
  }

  private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverDeckArea = DeckArea.GetRect().HasPoint(mousePosition);
    var mouseOverInventoryArea = InventoryArea.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverDeckArea && _draggedCardOrigin != CardOrigin.Deck) {
      if (_draggedCardState != DraggedCardState.OverDeckArea) {
        _draggedCardState = DraggedCardState.OverDeckArea;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverInventoryArea && _draggedCardOrigin != CardOrigin.Inventory) {
      if (_draggedCardState != DraggedCardState.OverInventoryArea) {
        _draggedCardState = DraggedCardState.OverInventoryArea;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (_stashEnabled && mouseOverStashArea && _draggedCardOrigin != CardOrigin.Stash) {
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
      case CardOrigin.Deck:
        Player.Deck.Remove(card);
        break;
      case CardOrigin.Inventory:
        Player.Inventory.Remove(card);
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
    var mouseOverDeckArea = DeckArea.GetRect().HasPoint(mousePosition);
    var mouseOverInventoryArea = InventoryArea.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverDeckArea && _draggedCardOrigin != CardOrigin.Deck) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      Player.Deck.Add(view.Card);
      FillContainersWithCardViews();
    }
    else if (mouseOverInventoryArea && _draggedCardOrigin != CardOrigin.Inventory) {
      if (Player.Inventory.Add(view.Card)) {
        RemoveDraggedCardFromItsOrigin(view.Card);
        FillContainersWithCardViews();
      }
    }
    else if (_stashEnabled && mouseOverStashArea && _draggedCardOrigin != CardOrigin.Stash) {
      RemoveDraggedCardFromItsOrigin(view.Card);
      Data.Stash.Add(view.Card);
      FillContainersWithCardViews();
    }

    UpdateLabels();
    _draggedCardOrigin = CardOrigin.None;
  }
}