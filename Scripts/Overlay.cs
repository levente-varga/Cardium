using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Overlay : Node2D {
  public struct OverlayTile {
    public Vector2I Position;
    public Color Color;
  }

  [Export] public Player Player = null!;
  
  private List<OverlayTile> _tiles = new();

  public List<OverlayTile> Tiles {
    get => _tiles;
    set {
      _tiles = new List<OverlayTile>(value);
      QueueRedraw();
    }
  }

  public int? Range;

  public override void _Ready() {
    Modulate = new Color(1, 1, 1, 0.3f);
  }

  public override void _Draw() {
    foreach (var tile in _tiles) {
      DrawRect(new Rect2(Global.TileToWorld(tile.Position), Global.GlobalTileSize), tile.Color);
    }
    
    DrawRangeBorder();

    base._Draw();
  }

  private void DrawRangeBorder() {
    if (Range == null) return;
    
    Vector2 center = Global.TileCenterToWorld(Player.Position);

    Vector2 up = center + Vector2.Up * Global.GlobalTileSize * Range.Value;
    Vector2 right = center + Vector2.Right * Global.GlobalTileSize * Range.Value;
    Vector2 down = center + Vector2.Down * Global.GlobalTileSize * Range.Value;
    Vector2 left = center + Vector2.Left * Global.GlobalTileSize * Range.Value;

    DrawLine(up, right, Colors.Red, 2.0f);
    DrawLine(right, down, Colors.Red, 2.0f);
    DrawLine(down, left, Colors.Red, 2.0f);
    DrawLine(left, up, Colors.Red, 2.0f);
  }
}