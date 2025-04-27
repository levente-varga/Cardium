using Godot;

namespace Cardium.Scripts.Menus;

public partial class VictoryMenu : Menu {
  [Export] public Player Player = null!;
  [Export] public Button CloseButton = null!;
  [Export] public LinkButton LinkButton = null!;

  public override void _Ready() {
    Visible = false;
    CloseButton.Pressed += Close;
  }

  public override void Close() {
    base.Close();
    
    Player.SaveCards();
    Data.LoadLobbyData();
    GetTree().ReloadCurrentScene();
  }
}