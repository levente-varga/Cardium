using System;
using System.Collections.Generic;
using Cardium.Scripts.Enemies;
using Godot;

namespace Cardium.Scripts;

public class Dungeon {
  private AStarGrid2D _grid = new();

  public TileMapLayer GroundLayer { get; private set; } = new();
  public TileMapLayer DecorLayer { get; private set; }= new();
  public TileMapLayer WallLayer { get; private set; } = new();
  public TileMapLayer ObjectLayer { get; private set; } = new();
  public TileMapLayer EnemyLayer { get; private set; } = new();
  public TileMapLayer LootLayer { get; private set; } = new();
  public TileMapLayer FogLayer { get; private set; } = new();
  public Overlay Overlay { get; private set; } = new();
  
  public Vector2I Size { get; private set; }
  
  public List<Enemy> Enemies { get; private set; } = new();

  // Data from the DungeonGenerator
  private List<List<bool>> _walls;
  private List<Rect2I> _rooms;

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

  private static bool LayerIsEmptyAt(TileMapLayer layer, Vector2I tile) {
    return layer.GetCellSourceId(tile) == -1;
  }

  public Dungeon(List<List<bool>> walls, List<Rect2I> rooms) {
    _walls = new(walls); // TODO: should deep copy
    _rooms = new(rooms);
    
    Size = new Vector2I(
      walls.Count > 0 ? walls[0].Count : 0,
      walls.Count
      );
    
    //WallLayer.Scale = new Vector2(4, 4);
    WallLayer.TileSet = ResourceLoader.Load<TileSet>("res://Assets/TileSets/walls.tres");
    
    GD.Print($"Instantiated a {Size.X}x{Size.Y} dungeon");
    
    for (var x = 0; x < _walls.Count; x++) {
      for (var y = 0; y < _walls[y].Count; y++) {
        //dungeon._grid.SetPointSolid(new Vector2I(x, y), walls[x][y]);
        WallLayer.SetCell(
          new Vector2I(x, y),
          1,
          _walls[x][y] ? new Vector2I(4, 0) : Vector2I.Zero
        );
      }
    }
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
    foreach (var room in _rooms) {
      Vector2I tile = new(
        _random.Next(room.Position.X, room.End.X - 1),
        _random.Next(room.Position.Y, room.End.Y - 1)
        );

      Slime enemy = new() {
        Position = tile
      };
      
      Enemies.Add(enemy);
    }
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