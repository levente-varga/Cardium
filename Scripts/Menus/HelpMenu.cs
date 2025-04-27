using Godot;

namespace Cardium.Scripts.Menus;

public partial class HelpMenu : Menu {
  [Export] public Button CloseButton = null!;

  public override void _Ready() {
    Visible = false;
    CloseButton.Pressed += Close;
  }
}