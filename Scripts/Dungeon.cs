using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public class Dungeon {
  private AStarGrid2D _grid = new();

  public TileMapLayer GroundLayer = new();
  public TileMapLayer DecorLayer = new();
  public TileMapLayer WallLayer = new();
  public TileMapLayer ObjectLayer = new();
  public TileMapLayer EnemyLayer = new();
  public TileMapLayer LootLayer = new();
  public TileMapLayer FogLayer = new();
  public Overlay Overlay = new();

  // Data from the DungeonGenerator
  public List<List<bool>> Walls;
  public List<Rect2I> Rooms;

  private readonly Dictionary<int, Vector2I> _bitmaskToWallAtlasCoord = new() {
    { 0b11010000, new Vector2I(0, 0) },
    { 0b01101000, new Vector2I(1, 0) },
  };
  
  int GetWallBitmask(int x, int y)
  {
    var mask = 0;

    if (IsWall(new Vector2I(x - 1, y - 1))) mask |= 128;
    if (IsWall(new Vector2I(x    , y - 1))) mask |= 1;
    if (IsWall(new Vector2I(x + 1, y - 1))) mask |= 2;
    if (IsWall(new Vector2I(x - 1, y    ))) mask |= 64;
    if (IsWall(new Vector2I(x + 1, y    ))) mask |= 4;
    if (IsWall(new Vector2I(x - 1, y + 1))) mask |= 32;
    if (IsWall(new Vector2I(x    , y + 1))) mask |= 16;
    if (IsWall(new Vector2I(x + 1, y + 1))) mask |= 8;

    return mask;
  }

  private bool IsWall(Vector2I cell) => WallLayer.GetCellSourceId(cell) != -1 
    || !new Rect2I(Vector2I.Zero, Size).HasPoint(cell);

  private Random _random = new ();

  private Vector2I _size = new(31, 31);

  public Vector2I Size {
    get => _size;
    set => SetSize(value);
  }

  private static bool LayerIsEmptyAt(TileMapLayer layer, Vector2I tile) {
    return layer.GetCellSourceId(tile) == -1;
  }

  public void SetSize(Vector2I size) {
    _size = new (
      Math.Max(1, size.X) / 2 * 2 + 1,
      Math.Max(1, size.Y) / 2 * 2 + 1
    );
  }

  public Dungeon() {
    //WallLayer.Scale = new Vector2(4, 4);
    WallLayer.TileSet = ResourceLoader.Load<TileSet>("res://Assets/TileSets/walls.tres");
  }

  public static Dungeon From(List<List<int>> walls) {
    var dungeon = new Dungeon();
    for (var x = 0; x < walls.Count; x++) {
      for (var y = 0; y < walls[x].Count; y++) {
        //dungeon._grid.SetPointSolid(new Vector2I(x, y), walls[x][y]);
        dungeon.WallLayer.SetCell(
          new Vector2I(x, y),
          1,
          walls[x][y] == -1
            ? new Vector2I(2, 3)
            : walls[x][y] == -2
              ? new Vector2I(2, 3)
              : new Vector2I(3, 3)
        );
      }
    }

    dungeon.PrettyWalls();

    return dungeon;
  }

  /// <summary>
  /// Sets wall sprites according to their surrounding cells
  /// </summary>
  private void PrettyWalls() {
    for (var x = 0; x < Size.X; x++) {
      for (var y = 0; y < Size.Y; y++) {
        if (LayerIsEmptyAt(WallLayer, new Vector2I(x, y))) continue;
        WallLayer.SetCell(
          coords: new Vector2I(x, y),
          sourceId: 1,
          atlasCoords: _bitmaskToWallAtlasCoord[GetWallBitmask(x, y)]
        );
      }
    }
  }

  private void Decorate() {
    for (var x = 0; x < Size.X; x++) {
      for (var y = 0; y < Size.Y; y++) {
        if (LayerIsEmptyAt(WallLayer, new Vector2I(x, y))) {
          if (_random.Next(10) == 0) {
            
          }
        }
      }
    }
  }

  private void SpawnEnemies() {
  }

  private void SpawnDoors() {
  }

  private void SpawnBonfires() {
  }

  private void SpawnChests() {
  }

  private void SpawnExits() {
  }
}