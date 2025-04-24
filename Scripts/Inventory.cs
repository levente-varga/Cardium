using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

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
	
	private const int CardsPerRow = 2;
	
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
			view.Draggable = false;
			view.HoverAnimation = CardView.HoverAnimationType.Grow;
			view.Position = Global.GlobalCardSize / 2;
			view.OnDragStartEvent += OnCardDragStartEventHandler;
			//view.OnDragEndEvent += OnCardDragEndEventHandler;
			view.OnDragEvent += OnCardDragEventHandler;
			view.OnPressedEvent += OnCardPressedEventHandler;
			cardContainer.AddChild(view);
			row.AddChild(cardContainer);
			
			storage.Add(view);

			if (i != cards.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
			row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
			container.AddChild(row);
			row = new HBoxContainer();
		}
	}

	private void OnCardDragStartEventHandler(CardView view) {
		
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
	
	private void OnCardDragEventHandler(CardView view, Vector2 mousePosition) {
		
	}

	private void OnCardDragEndEventHandler(CardView view, Vector2 mousePosition) {
		if (_cardsInInventory.Contains(view) && DeckArea.GetRect().HasPoint(mousePosition)) {
			// TODO: Move dragged card to deck
			Player.Inventory.Remove(view.Card);
			Player.Deck.Add(view.Card);
			FillContainersWithCardViews();
		}
		else if (_cardsInDeck.Contains(view) && InventoryArea.GetRect().HasPoint(mousePosition)) {
			// TODO: Move dragged card to inventory
			Player.Deck.Remove(view.Card);
			Player.Inventory.Add(view.Card);
			FillContainersWithCardViews();
		}
	}
}