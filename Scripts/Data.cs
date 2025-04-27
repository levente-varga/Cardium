namespace Cardium.Scripts;

public enum Level {
  Lobby,
  One,
}

public static class Data {
  public static Level Level;
  public static bool InitialStart = true;
  public static bool Fog;
  public static bool Hand;
  public static bool ShowHealth;
  public static bool CameraOnPlayer;
  public static int MenusOpen;
  public static bool MenuOpen => MenusOpen > 0;
  public static readonly Pile Stash = new Pile();
  public static readonly Pile Inventory = new Pile();
  public static readonly Pile Deck = new Pile();

  public static void LoadLobbyData() {
    Level = Level.Lobby;
    Fog = false;
    Hand = false;
    ShowHealth = false;
    CameraOnPlayer = false;
    MenusOpen = 0;
  }

  public static void LoadDungeonData() {
    Level = Level.One;
    Fog = true;
    Hand = true;
    ShowHealth = true;
    CameraOnPlayer = true;
    MenusOpen = 0;
  }
}