using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Menus;

public partial class InventoryMenu : Menu {
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
  [Export] public Button StashToInventoryButton = null!;
  [Export] public Button StashToDeckButton = null!;
  [Export] public Button InventoryToStashButton = null!;
  [Export] public Button InventoryToDeckButton = null!;
  [Export] public Button DeckToStashButton = null!;
  [Export] public Button DeckToInventoryButton = null!;

  [Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

  private bool _stashEnabled;

  private DraggedCardState _draggedCardState;

  private const int CardsPerRow = 2;

  public override void _Ready() {
    Visible = false;
    OkButton.Pressed += Close;
    StashToInventoryButton.Pressed += () => MoveCards(Data.Stash, Data.Inventory, Card.Origins.Inventory);
    StashToDeckButton.Pressed += () => MoveCards(Data.Stash, Data.Deck, Card.Origins.Deck);
    InventoryToStashButton.Pressed += () => MoveCards(Data.Inventory, Data.Stash, Card.Origins.Stash);
    InventoryToDeckButton.Pressed += () => MoveCards(Data.Inventory, Data.Deck, Card.Origins.Deck);
    DeckToStashButton.Pressed += () => MoveCards(Data.Deck, Data.Stash, Card.Origins.Stash);
    DeckToInventoryButton.Pressed += () => MoveCards(Data.Deck, Data.Inventory, Card.Origins.Inventory);
  }

  public override void _Input(InputEvent @event) {
    if (!Visible) return;
    
    if (InputMap.EventIsAction(@event, "Back") && @event.IsPressed()) {
      Close();
      GetViewport().SetInputAsHandled();
    }
  }

  public void Open(bool enableStash = false) {
    base.Open();
    _stashEnabled = enableStash;
    Player.PutCardsInUseIntoDeck();
    Player.SaveCards();
    FillContainersWithCardViews();
    UpdateLabels();
    if (!_stashEnabled) StashArea.Color = new Color("16161688");
  }

  public override void Close() {
    base.Close();
    Player.LoadCards();
    Player.Hand.DrawUntilFull();
    _stashEnabled = false;
    StashArea.Color = new Color("16161600");
  }

  private void FillContainersWithCardViews() {
    FillContainerWithCardViews(StashContainer, Data.Stash.GetCardsSorted());
    FillContainerWithCardViews(InventoryContainer, Data.Inventory.GetCardsSorted());
    FillContainerWithCardViews(DeckContainer, Data.Deck.GetCardsSorted());
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
      view.OnDragEndEvent += OnCardDragEndEventHandler;
      view.OnDragEvent += OnCardDragEventHandler;
      view.ShowOrigin = false;
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
    InventorySizeLabel.Text = $"({Data.Inventory.Size})";
    DeckSizeLabel.Text = $"({Data.Deck.Size} / {Data.Deck.Capacity})";
  }

  private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverDeckArea = DeckArea.GetRect().HasPoint(mousePosition);
    var mouseOverInventoryArea = InventoryArea.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverDeckArea && view.Card.Origin != Card.Origins.Deck) {
      if (_draggedCardState != DraggedCardState.OverDeckArea) {
        _draggedCardState = DraggedCardState.OverDeckArea;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (mouseOverInventoryArea && view.Card.Origin != Card.Origins.Inventory) {
      if (_draggedCardState != DraggedCardState.OverInventoryArea) {
        _draggedCardState = DraggedCardState.OverInventoryArea;
        view.PlayScaleAnimation(0.75f);
      }
    }
    else if (_stashEnabled && mouseOverStashArea && view.Card.Origin != Card.Origins.Stash) {
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
    switch (card.Origin) {
      case Card.Origins.Deck:
        Data.Deck.Remove(card);
        break;
      case Card.Origins.Inventory:
        Data.Inventory.Remove(card);
        break;
      case Card.Origins.Stash:
        Data.Stash.Remove(card);
        break;
      case Card.Origins.None:
      default:
        break;
    }
  }

  private void OnCardDragEndEventHandler(CardView view, Vector2 mousePosition) {
    var mouseOverDeckArea = DeckArea.GetRect().HasPoint(mousePosition);
    var mouseOverInventoryArea = InventoryArea.GetRect().HasPoint(mousePosition);
    var mouseOverStashArea = StashArea.GetRect().HasPoint(mousePosition);

    if (mouseOverDeckArea && view.Card.Origin != Card.Origins.Deck) {
      if (Data.Deck.Add(view.Card)) {
        RemoveDraggedCardFromItsOrigin(view.Card);
        view.Card.Origin = Card.Origins.Deck;
        FillContainersWithCardViews();
      }
    }
    else if (mouseOverInventoryArea && view.Card.Origin != Card.Origins.Inventory) {
      if (Data.Inventory.Add(view.Card)) {
        RemoveDraggedCardFromItsOrigin(view.Card);
        view.Card.Origin = Card.Origins.Inventory;
        FillContainersWithCardViews();
      }
    }
    else if (_stashEnabled && mouseOverStashArea && view.Card.Origin != Card.Origins.Stash) {
      if (Data.Stash.Add(view.Card)) {
        RemoveDraggedCardFromItsOrigin(view.Card);
        view.Card.Origin = Card.Origins.Stash;
        FillContainersWithCardViews();
      }
    }

    UpdateLabels();
  }

  private void MoveCards(Pile from, Pile to, Card.Origins newOrigin) {
    var cards = new List<Card>(from.Cards);
    foreach (var card in cards) {
      if (!to.Add(card)) break;
      card.Origin = newOrigin;
      from.Remove(card);
    }
    UpdateLabels();
    FillContainersWithCardViews();
  } 
}