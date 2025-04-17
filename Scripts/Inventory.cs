using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Inventory : Control {
	[Export] public Player Player = null!;
	[Export] public VBoxContainer DeckContainer = null!;
	[Export] public VBoxContainer InventoryContainer = null!;
	
	[Export] private PackedScene _cardScene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn");

	private const int CardsPerRow = 2;
	
	public override void _Ready() {
		Visible = false;
		var cardsInDeck = Player.Deck.Cards;
		var row = new HBoxContainer();

		if (DeckContainer.GetParent() is MarginContainer parent) {
			parent.AddThemeConstantOverride("margin_top", 24);
			parent.AddThemeConstantOverride("margin_left", 24);
			parent.AddThemeConstantOverride("margin_bottom", 24);
			parent.AddThemeConstantOverride("margin_right", 24);	
		}
		
		for (var i = 0; i < cardsInDeck.Count; i++) {
			var card = cardsInDeck[i];
			var rowNumber = i / CardsPerRow;

			var container = new Container();
			container.SetCustomMinimumSize(Global.GlobalCardSize);
			var view = _cardScene.Instantiate<CardView>();
			view.Init(card, false);
			view.Position = Global.GlobalCardSize / 2;
			container.AddChild(view);
			row.AddChild(container);

			if (i != cardsInDeck.Count - 1 && rowNumber == (i + 1) / CardsPerRow) continue;
			row.SetCustomMinimumSize(new Vector2(Global.CardSize.X * CardsPerRow, Global.CardSize.Y));
			DeckContainer.AddChild(row);
			row = new();
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