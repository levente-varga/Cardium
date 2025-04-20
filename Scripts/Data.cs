namespace Cardium.Scripts;

public enum Level {
  Lobby,
  One,
}

public class Data {
  public static Level Level;
  public static bool Fog;

  public static void LoadLobbyData() {
    Level = Level.Lobby;
    Fog = false;
  }

  public static void LoadDungeonData() {
    Level = Level.One;
    Fog = true;
  }
}