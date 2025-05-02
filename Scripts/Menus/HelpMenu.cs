using Godot;

namespace Cardium.Scripts.Menus;

public partial class HelpMenu : Menu {
  [Export] public Button CloseButton = null!;

  public override void _Ready() {
    Visible = true;
    CloseButton.Pressed += Close;
  }
  
  public override void _Input(InputEvent @event) {
    if (!Visible) return;
    
    if (InputMap.EventIsAction(@event, "Back") && @event.IsPressed()) {
      Close();
      GetViewport().SetInputAsHandled();
    }
  }
}