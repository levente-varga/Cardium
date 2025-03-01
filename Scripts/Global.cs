using Godot;

namespace Cardium.Scripts;

public static class Global
{
    public static readonly Vector2I SpriteSize = new (16, 16);
    public const int Scale = 4;
    public static Vector2I TileSize => SpriteSize * Scale;
    public const float LerpWeight = 15f;
}