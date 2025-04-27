using Godot;

namespace Cardium.Scripts.Menus;

public partial class DeathMenu : Menu {
  [Export] public Player Player = null!;
  [Export] public Button CloseButton = null!;

  public override void _Ready() {
    Visible = false;
    CloseButton.Pressed += Close;
  }

  public override void Close() {
    base.Close();
    
    Player.SaveCards();
    Data.EraseUnprotectedCardsOutsideStash();
    Data.LoadLobbyData();
    GetTree().ReloadCurrentScene();
  }
}