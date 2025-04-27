using Godot;

namespace Cardium.Scripts.Menus;

public partial class PauseMenu : Menu {
  [Export] public Player Player = null!;
  [Export] public Button ResumeButton = null!;
  [Export] public Button FleeButton = null!;
  [Export] public Button QuitButton = null!;

  public override void _Ready() {
    Visible = false;
    ResumeButton.Pressed += Close;
    FleeButton.Pressed += Flee;
    QuitButton.Pressed += Quit;
  }

  public override void Open() {
    base.Open();

    FleeButton.Disabled = Data.Level == Level.Lobby;
  }

  public void Flee() {
    base.Close();
    
    Player.SaveCards();
    Data.EraseUnprotectedCardsOutsideStash();
    Data.LoadLobbyData();
    GetTree().ReloadCurrentScene();
  }
  
  public void Quit() {
    Player.GetTree().Quit();
  }
}