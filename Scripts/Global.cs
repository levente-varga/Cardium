using Godot;

namespace Cardium.Scripts;

public static class Global
{
    public static readonly Vector2I SpriteSize = new (16, 16);
    public const int Scale = 4;
    public static Vector2I TileSize => SpriteSize * Scale;
    public const float LerpWeight = 15f;
    
    public static readonly Vector2I ChestAtlasCoords = new (8, 6);
    public static readonly Vector2I BonfireAtlasCoords = new (14, 10);
    public static readonly Vector2I DoorAtlasCoords = new (3, 9);
    
    public static readonly Vector2I SlimeAtlasCoords = new (27, 8);
    public static readonly Vector2I SpiderAtlasCoords = new (28, 5);
    
    public static readonly Color Yellow = new ("F4B41B");
    public static readonly Color Red = new ("E6482E");
    public static readonly Color White = new ("FFFFFF");
    public static readonly Color Black = new ("000000");
    public static readonly Color Magenta = new ("FF23D9");
    public static readonly Color Purple = new ("7D3CD7");
    public static readonly Color Green = new ("38D973");
}