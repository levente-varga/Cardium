using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Inventory : Control {
	[Export] public Player Player = null!;
	[Export] public VBoxContainer DeckContainer = null!;
	[Export] public VBoxContainer InventoryContainer = null!;
	
	[Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

	private const int CardsPerRow = 2;

	public new bool Visible {
		get => base.Visible;
		set {
			base.Visible = value;
			if (value) FillContainersWithCardViews();
		}
	}
	
	public override void _Ready() {
		Visible = false;
	}

	private void FillContainersWithCardViews() {
		List<Card> cardsInUse = new(Player.Hand.Deck.Deck.GetCards()
			.Union(Player.Hand.DiscardPile.Pile.GetCards()
				.Union(Player.Hand.GetCards())));
		
		FillContainerWithCardViews(DeckContainer, cardsInUse);
		FillContainerWithCardViews(InventoryContainer, Player.Inventory.GetCards());
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
			view.Init(card, false);
			view.Position = Global.GlobalCardSize / 2;
			cardContainer.AddChild(view);
			row.AddChild(cardContainer);

			if (i != cards.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
			row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
			container.AddChild(row);
			row = new HBoxContainer();
		}
	}

	public override void _Process(double delta) {
		
	}

	public override void _Input(InputEvent @event) {
		if (!@event.IsPressed()) return;

		if (InputMap.EventIsAction(@event, "ToggleInventoryMenu")) {
			Visible = !Visible;
		}
	}
}