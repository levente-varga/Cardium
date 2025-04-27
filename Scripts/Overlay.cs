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
    
  }

  public override void _Draw() {
    foreach (var tile in _tiles) {
      DrawRect(new Rect2(Global.TileToWorld(tile.Position), Global.GlobalTileSize), new Color(tile.Color.R, tile.Color.G, tile.Color.B, 0.3f));
    }
    
    DrawRangeBorder();

    base._Draw();
  }

  private void DrawRangeBorder() {
    if (Range is null or <= 0) return;
    
    var current = Global.TileToWorld(Player.Position) + Vector2.Up * Range.Value * Global.GlobalTileSize;;
    Vector2 next;

    var right = Vector2.Right * Global.GlobalTileSize;
    var left = Vector2.Left * Global.GlobalTileSize;
    var up = Vector2.Up * Global.GlobalTileSize;
    var down = Vector2.Down * Global.GlobalTileSize;

    var edgesPerSide = Range.Value * 2 + 1;

    for (var i = 0; i < edgesPerSide; i++) {
      next = current + (i % 2 == 0 ? right : down);
      DrawLine(current, next, Colors.White, 4);
      current = next;
    }

    for (var i = 0; i < edgesPerSide; i++) {
      next = current + (i % 2 == 0 ? down : left);
      DrawLine(current, next, Colors.White, 4);
      current = next;
    }

    for (var i = 0; i < edgesPerSide; i++) {
      next = current + (i % 2 == 0 ? left : up);
      DrawLine(current, next, Colors.White, 4);
      current = next;
    }
    
    for (var i = 0; i < edgesPerSide; i++) {
      next = current + (i % 2 == 0 ? up : right);
      DrawLine(current, next, Colors.White, 4);
      current = next;
    }
  }
}