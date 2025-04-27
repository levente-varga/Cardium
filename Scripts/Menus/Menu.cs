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
}