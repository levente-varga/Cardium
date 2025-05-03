using System;
using Godot;

namespace Cardium.Scripts.Menus;

public partial class PauseMenu : Menu {
  [Export] public Player Player = null!;
  [Export] public Button ResumeButton = null!;
  [Export] public Button HelpButton = null!;
  [Export] public Button FleeButton = null!;
  [Export] public Button QuitButton = null!;
  [Export] public HelpMenu HelpMenu = null!;
  [Export] public Label QuitLabel = null!;

  public override void _Ready() {
    Visible = false;
    ResumeButton.Pressed += Close;
    HelpButton.Pressed += HelpMenu.Open;
    FleeButton.Pressed += Flee;
    QuitButton.Pressed += Quit;
  }

  public override void Open() {
    base.Open();

    Data.PauseMenuOpen = true;
    QuitLabel.Text = $"Saved {Utils.GetMinutesBetween(Data.LastSaveTime, DateTime.Now)} mins ago";
    FleeButton.Disabled = Data.Level == Level.Lobby;
  }
  
  public override void Close() {
    base.Close();

    Data.PauseMenuOpen = false;
  }

  private void Flee() {
    Close();
    
    Player.SaveCards();
    Data.EraseUnprotectedCardsOutsideStash();
    Data.LoadLobbyData();
    GetTree().ReloadCurrentScene();
  }

  private void Quit() {
    Player.GetTree().Quit();
  }
}