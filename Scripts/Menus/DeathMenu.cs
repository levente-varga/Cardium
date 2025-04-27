using Godot;

namespace Cardium.Scripts.Menus;

public partial class DeathMenu : Control {
  [Export] public Player Player = null!;
  [Export] public Button CloseButton = null!;

  public override void _Ready() {
    Visible = false;
    CloseButton.Pressed += OkButtonPressed;
  }

  public override void _Process(double delta) {
  }

  public override void _Input(InputEvent @event) {
    if (!Visible) return;
  }

  public void Open() {
    Visible = true;
    Data.MenuOpen = true;
  }

  private void OkButtonPressed() {
    Close();
  }

  private void Close() {
    Visible = false;
    Data.MenuOpen = false;
  }
}