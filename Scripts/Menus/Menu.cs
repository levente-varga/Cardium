using Godot;

namespace Cardium.Scripts.Menus;

public partial class Menu : Control {
  public override void _Ready() {
    Visible = false;
  }
  
  public virtual void Open() {
    Visible = true;
    Data.MenusOpen++;
  }

  public virtual void Close() {
    Visible = false;
    Data.MenusOpen--;
  }
  
  public override void _Input(InputEvent @event) {
    if (!Visible) return;
    
    if (InputMap.EventIsAction(@event, "Back") && @event.IsPressed()) {
      Close();
      GetViewport().SetInputAsHandled();
    }
  }
}