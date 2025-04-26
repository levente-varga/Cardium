using Godot;

namespace Cardium.Scripts;

public partial class CharacterPanel : Control {
  [Export] public Label HealthLabel = null!;
  [Export] public Label DefenseLabel = null!;
  [Export] public Label VisionLabel = null!;
  [Export] public Label HandLabel = null!;
  [Export] public Label DeckLabel = null!;
  [Export] public Label InventoryLabel = null!;
  [Export] public Player Player = null!;

  public override void _Ready() {
  }

  public override void _Process(double delta) {
    if (!Visible) return;

    HealthLabel.Text = $"{Player.Health} / {Player.MaxHealth}";
    DefenseLabel.Text = $"{Player.Armor}";
    VisionLabel.Text = $"{Player.Vision}";
    HandLabel.Text = $"{Player.Hand.Size} / {Player.Hand.Capacity}";
    DeckLabel.Text = $"{Player.Deck.Deck.Size} / {Player.Deck.Deck.Capacity}";
    InventoryLabel.Text = $"{Player.Inventory.Size}";
  }

  public override void _Input(InputEvent @event) {
    if (!@event.IsPressed()) return;

    if (InputMap.EventIsAction(@event, "ToggleCharacterMenu")) {
      Visible = !Visible;
    }
  }
}