namespace Cardium.Scripts;

public enum Level {
  Lobby,
  One,
}

public class Data {
  public static Level Level;
  public static bool Fog;
  public static bool Hand;
  public static bool ShowHealth;
  public static bool CameraOnPlayer;

  public static void LoadLobbyData() {
    Level = Level.Lobby;
    Fog = false;
    Hand = false;
    ShowHealth = false;
    CameraOnPlayer = false;
  }

  public static void LoadDungeonData() {
    Level = Level.One;
    Fog = true;
    Hand = true;
    ShowHealth = true;
    CameraOnPlayer = true;
  }
}