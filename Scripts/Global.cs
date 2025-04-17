using Godot;

namespace Cardium.Scripts;

public static class Global {
  public static Vector2I SpriteSize => new (16, 16);
  public const int Scale = 4;
  public const int CardScale = 6;
  public static Vector2I CardSize => new (38, 54);
  public static Vector2I GlobalCardSize => CardSize * CardScale;
  public static Vector2I GlobalTileSize => SpriteSize * Scale;
  public static Vector2 TileToWorld(Vector2I tile) => tile * GlobalTileSize;
  public const float LerpWeight = 15f;

  public static Color Yellow => new ("F4B41B");
  public static Color Red => new ("E6482E");
  public static Color White => new ("FFFFFF");
  public static Color Black => new ("000000");
  public static Color Magenta => new ("FF23D9");
  public static Color Purple => new ("7D3CD7");
  public static Color Green => new ("38D973");

  public static bool Debug = false;
}