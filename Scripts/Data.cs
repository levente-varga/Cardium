namespace Cardium.Scripts;

public enum Level {
  Lobby,
  One,
}

public static class Data {
  public static Level Level;
  public static bool InitialStart = true;
  public static int InitialCardPlaysLeft = 1;
  public static bool Fog;
  public static bool Hand;
  public static bool ShowHealth;
  public static bool CameraOnPlayer;
  public static int MenusOpen;
  public static bool MenuOpen => MenusOpen > 0;
  public static readonly Pile Stash = new ();
  public static readonly Pile Inventory = new ();
  public static readonly Pile Deck = new ();

  public static void EraseUnprotectedCardsOutsideStash() {
    Inventory.RemoveUnprotected();
    Deck.RemoveUnprotected();
  }

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