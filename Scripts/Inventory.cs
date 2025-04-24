using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public enum CardOrigin { None, Deck, Stash, Inventory, }
public enum DraggedCardState { None, OverDeckArea, OverInventoryArea, OverStashArea }

public partial class Inventory : Control {
	[Export] public Player Player = null!;
	[Export] public VBoxContainer DeckContainer = null!;
	[Export] public VBoxContainer InventoryContainer = null!;
	[Export] public Control DeckArea = null!;
	[Export] public Control InventoryArea = null!;
	[Export] public Button OkButton = null!;
	
	[Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

	private readonly List<CardView> _cardsInInventory = new();
	private readonly List<CardView> _cardsInDeck = new();
	
	private DraggedCardState _draggedCardState;
	
	private const int CardsPerRow = 2;
	private CardOrigin _draggedCardOrigin;
	
	public override void _Ready() {
		Visible = false;
		OkButton.Pressed += OkButtonPressed;
	}
	
	public override void _Process(double delta) {
		
	}
	
	public override void _Input(InputEvent @event) {
		if (!Visible) return;
	}
	
	public void Open() {
		Visible = true;
		Player.PutCardsInUseIntoDeck();
		FillContainersWithCardViews();
	}

	public void OkButtonPressed() {
		Player.Hand.DrawUntilFull();
		Visible = false;
	}

	private void FillContainersWithCardViews() {
		List<Card> cardsInUse = new(Player.Deck.Deck.GetCards());
		
		FillContainerWithCardViews(DeckContainer, cardsInUse, _cardsInDeck);
		FillContainerWithCardViews(InventoryContainer, Player.Inventory.GetCards(), _cardsInInventory);
	}

	private void FillContainerWithCardViews(Container container, List<Card> cards, List<CardView> storage) {
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
			view.Enabled = true;
			view.Draggable = true;
			view.HoverAnimation = CardView.HoverAnimationType.Grow;
			view.Position = Global.GlobalCardSize / 2;
			view.OnDragStartEvent += OnCardDragStartEventHandler;
			view.OnDragEndEvent += OnCardDragEndEventHandler;
			view.OnDragEvent += OnCardDragEventHandler;
			//view.OnPressedEvent += OnCardPressedEventHandler;
			cardContainer.AddChild(view);
			row.AddChild(cardContainer);
			
			storage.Add(view);

			if (i != cards.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
			row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
			container.AddChild(row);
			row = new HBoxContainer();
		}
	}

	private void OnCardPressedEventHandler(CardView view) {
		if (_cardsInInventory.Contains(view)) {
			// TODO: Move dragged card to deck
			Player.Inventory.Remove(view.Card);
			Player.Deck.Add(view.Card);
			FillContainersWithCardViews();
			GD.Print($"Moved {view.Card.Name}: Inventory -> Deck");
		}
		else if (_cardsInDeck.Contains(view)) {
			// TODO: Move dragged card to inventory
			Player.Deck.Remove(view.Card);
			Player.Inventory.Add(view.Card);
			FillContainersWithCardViews();
			GD.Print($"Moved {view.Card.Name}: Deck -> Inventory");

		}
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
		GD.Print($"Started dragging a card from {_draggedCardOrigin}");
	}
	
	private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
		var mouseOverDeckArea = DeckArea.GetRect().HasPoint(mousePosition);
		var mouseOverInventoryArea = InventoryArea.GetRect().HasPoint(mousePosition);
		var mouseOverStashArea = false;
		
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
		GD.Print($"Removing card originating from {_draggedCardOrigin}");
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
		var mouseOverStashArea = false;
		
		if (mouseOverDeckArea && _draggedCardOrigin != CardOrigin.Deck) {
			RemoveDraggedCardFromItsOrigin(view.Card);
			Player.Deck.Add(view.Card);
			FillContainersWithCardViews();
		}
		else if (mouseOverInventoryArea && _draggedCardOrigin != CardOrigin.Inventory) {
			RemoveDraggedCardFromItsOrigin(view.Card);
			Player.Inventory.Add(view.Card);
			FillContainersWithCardViews();
		}
		else if (mouseOverStashArea && _draggedCardOrigin != CardOrigin.Stash) {
			RemoveDraggedCardFromItsOrigin(view.Card);
			Data.Stash.Add(view.Card);
			FillContainersWithCardViews();
		}
		
		_draggedCardOrigin = CardOrigin.None;
	}
}