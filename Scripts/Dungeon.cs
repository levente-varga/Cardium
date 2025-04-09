using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Enemies;
using Godot;

namespace Cardium.Scripts;

public enum Tiles {
  Wall,
  RoomInterior,
  RoomPerimeter,
  RoomCorner,
  RoomEntrance,
  Corridor,
}

public class Dungeon {
  private AStarGrid2D _grid = new();

  public TileMapLayer GroundLayer { get; private set; } = new();
  public TileMapLayer DecorLayer { get; private set; } = new();
  public TileMapLayer WallLayer { get; private set; } = new();
  public TileMapLayer FogLayer { get; private set; } = new();
  public Overlay Overlay { get; private set; } = new();
  
  public Vector2I Size { get; private set; }
  public Rect2I Rect { get; private set; }
  
  public List<Enemy> Enemies { get; private set; } = new();
  public List<Interactable> Interactables { get; private set; } = new();
  public List<Card> Loot { get; private set; } = new();

  // Data from the DungeonGenerator
  private List<List<Tiles>> _tiles;
  private List<Rect2I> _rooms;

  private readonly Dictionary<int, Vector2I> _bitmaskToWallAtlasCoord = new();

  private Random _random = new ();

  public Dungeon(List<List<Tiles>> tiles, List<Rect2I> rooms) {
    _tiles = new List<List<Tiles>>();
    foreach (var row in tiles) {
      _tiles.Add(new List<Tiles>(row));
    }
    _rooms = new List<Rect2I>(rooms);
    
    // Assuming the received list of lists (tiles) has uniform length
    Size = new Vector2I(
      _tiles.Count,
      _tiles.Count > 0 ? _tiles[0].Count : 0
      );
    Rect = new Rect2I(Vector2I.Zero, Size);
    
    WallLayer.Scale = new Vector2(4, 4);
    DecorLayer.Scale = new Vector2(4, 4);
    
    WallLayer.TileSet = ResourceLoader.Load<TileSet>("res://Assets/TileSets/walls.tres");
    DecorLayer.TileSet = ResourceLoader.Load<TileSet>("res://Assets/TileSets/decor.tres");
    
    GD.Print($"Instantiated a {Size.X}x{Size.Y} dungeon");
    
    for (var x = 0; x < Size.X; x++) {
      for (var y = 0; y < Size.Y; y++) {
        //dungeon._grid.SetPointSolid(new Vector2I(x, y), walls[x][y]);
        if (_tiles[x][y] == Tiles.Wall) WallLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(4, 0));
      }
    }
    
    FillBitmaskDictionary();
    PrettyWalls();
    Decorate();
    SpawnEnemies();
  }

  /// <summary>
  /// Sets wall sprites according to their surrounding cells
  /// </summary>
  private void PrettyWalls() {
    for (var x = 0; x < Size.X; x++) {
      for (var y = 0; y < Size.Y; y++) {
        var cell = new Vector2I(x, y);
        if (LayerIsEmptyAt(WallLayer, cell)) continue;
        var atlasCoords = WallLayer.GetCellAtlasCoords(cell);
        int bitmask = GetWallBitmask(x, y);
        if (_bitmaskToWallAtlasCoord.Keys.Contains(bitmask)) {
          atlasCoords = _bitmaskToWallAtlasCoord[bitmask];
        }
        WallLayer.SetCell(cell, 0, atlasCoords);
      }
    }
  }

  private void Decorate() {
    for (var x = 0; x < Size.X; x++) {
      for (var y = 0; y < Size.Y; y++) {
        if (LayerIsEmptyAt(WallLayer, new Vector2I(x, y))) {
          float factor = 1;
          if (_tiles[x][y] == Tiles.RoomCorner) factor = 0.4f;
          else if (_tiles[x][y] == Tiles.RoomPerimeter) factor = 0.8f;
          else if (_tiles[x][y] == Tiles.RoomInterior) factor = 1.2f;
          
          var random = _random.Next(100);
          random = (int)(random * factor);
          
          if (random == 0) {
            DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(8, 0));
          }
          else if (random <= 2) {
            DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(5, 0));
          }
          else if (random <= 6) {
            DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(2, 0));
          }
          else if (random <= 18) {
            DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0));
          }
        }
      }
    }
  }

  private void SpawnEnemies() {
    foreach (var room in _rooms) {
      Vector2I tile = new(
        _random.Next(room.Position.X + 1, room.End.X - 2),
        _random.Next(room.Position.Y + 1, room.End.Y - 2)
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

  /// <summary>
  /// Adds key-value pairs to the [_bitmaskToWallAtlasCoord] dictionary.
  /// </summary>
  /// <param name="pattern">
  /// Defines the rules for the generated bitmasks. Each list item corresponds to
  /// one bit in the bitmask (first item is the largest bit). If a value is true, its bit
  /// must be 1, if false, 0. If null that bit is a variable. All bitmask that fit to this
  /// pattern will be generated. The list's length must be 8.
  /// </param>
  /// <param name="value">The value assigned to each key-value pair.</param>
  /// <exception cref="ArgumentException">The list's length must be 8.</exception>
  private void AddBitmaskEntries(List<bool?> pattern, Vector2I value) {
    if (pattern.Count != 8)
      throw new ArgumentException("Pattern must have exactly 8 elements.");

    pattern.Reverse();
    
    void Generate(int index, byte bitmask) {
      if (index == 8) {
        if (_bitmaskToWallAtlasCoord.Keys.Contains(bitmask))
          throw new Exception($"Dictionary already contains generated key: {bitmask}");
        _bitmaskToWallAtlasCoord[bitmask] = value;
        return;
      }

      if (pattern[index] == null || pattern[index] == true)
        Generate(index + 1, (byte)(bitmask | (1 << index)));
      if (pattern[index] == null || pattern[index] == false)
        Generate(index + 1, bitmask);
    }

    Generate(0, 0);
  }

  private static bool LayerIsEmptyAt(TileMapLayer layer, Vector2I tile) => layer.GetCellSourceId(tile) == -1;
  
  private bool IsWall(Vector2I cell) => WallLayer.GetCellSourceId(cell) != -1 || !Rect.HasPoint(cell);
  
  private int GetWallBitmask(int x, int y) {
    var mask = 0;
    if (IsWall(new Vector2I(x - 1, y - 1))) mask |= 128;
    if (IsWall(new Vector2I(x    , y - 1))) mask |= 64;
    if (IsWall(new Vector2I(x + 1, y - 1))) mask |= 32;
    if (IsWall(new Vector2I(x - 1, y    ))) mask |= 16;
    if (IsWall(new Vector2I(x + 1, y    ))) mask |= 8;
    if (IsWall(new Vector2I(x - 1, y + 1))) mask |= 4;
    if (IsWall(new Vector2I(x    , y + 1))) mask |= 2;
    if (IsWall(new Vector2I(x + 1, y + 1))) mask |= 1;
    return mask;
  }
  
  private void FillBitmaskDictionary() {
    //                                  TL     T      TR     L      R      BL     B      BR
    // 4 walls (1)
    AddBitmaskEntries(new () {null , false, null , false, false, null , false, null }, new Vector2I(4, 0));
    // 3 walls (4)
    AddBitmaskEntries(new () {null , true , null , false, false, null , false, null }, new Vector2I(5, 2));
    AddBitmaskEntries(new () {null , false, null , true , false, null , false, null }, new Vector2I(3, 0));
    AddBitmaskEntries(new () {null , false, null , false, true , null , false, null }, new Vector2I(1, 0));
    AddBitmaskEntries(new () {null , false, null , false, false, null , true , null }, new Vector2I(5, 0));
    // 2 walls - straight (2)
    AddBitmaskEntries(new () {null , true , null , false, false, null , true , null }, new Vector2I(5, 1));
    AddBitmaskEntries(new () {null , false, null , true , true , null , false, null }, new Vector2I(2, 0));
    // 2 walls - corners (8)
    AddBitmaskEntries(new () {false, true , null , true , false, null , false, null }, new Vector2I(4, 5));
    AddBitmaskEntries(new () {true , true , null , true , false, null , false, null }, new Vector2I(3, 4));
    AddBitmaskEntries(new () {null , true , false, false, true , null , false, null }, new Vector2I(0, 5));
    AddBitmaskEntries(new () {null , true , true , false, true , null , false, null }, new Vector2I(1, 4));
    AddBitmaskEntries(new () {null , false, null , true , false, false, true , null }, new Vector2I(4, 1));
    AddBitmaskEntries(new () {null , false, null , true , false, true , true , null }, new Vector2I(3, 2));
    AddBitmaskEntries(new () {null , false, null , false, true , null , true , false}, new Vector2I(0, 1));
    AddBitmaskEntries(new () {null , false, null , false, true , null , true , true }, new Vector2I(1, 2));
    // 1 wall - 0 corner (4)
    AddBitmaskEntries(new () {null , false, null , true , true , true , true , true }, new Vector2I(2, 2));
    AddBitmaskEntries(new () {null , true , true , false, true , null , true , true }, new Vector2I(1, 3));
    AddBitmaskEntries(new () {true , true , null , true , false, true , true , null }, new Vector2I(3, 3));
    AddBitmaskEntries(new () {true , true , true , true , true , null , false, null }, new Vector2I(2, 4));
    // 1 wall - 1 corner (8)
    AddBitmaskEntries(new () {null , false, null , true , true , false, true , true }, new Vector2I(3, 1));
    AddBitmaskEntries(new () {null , false, null , true , true , true , true , false}, new Vector2I(1, 1));
    AddBitmaskEntries(new () {null , true , false, false, true , null , true , true }, new Vector2I(0, 4));
    AddBitmaskEntries(new () {null , true , true , false, true , null , true , false}, new Vector2I(0, 2));
    AddBitmaskEntries(new () {false, true , null , true , false, true , true , null }, new Vector2I(4, 4));
    AddBitmaskEntries(new () {true , true , null , true , false, false, true , null }, new Vector2I(4, 2));
    AddBitmaskEntries(new () {false, true , true , true , true , null , false, null }, new Vector2I(3, 5));
    AddBitmaskEntries(new () {true , true , false, true , true , null , false, null }, new Vector2I(1, 5));
    // 1 wall - 2 corners (4)
    AddBitmaskEntries(new () {null , false, null , true , true , false, true , false}, new Vector2I(2, 1));
    AddBitmaskEntries(new () {null , true , false, false, true , null , true , false}, new Vector2I(0, 3));
    AddBitmaskEntries(new () {false, true , null , true , false, false, true , null }, new Vector2I(4, 3));
    AddBitmaskEntries(new () {false, true , false, true , true , null , false, null }, new Vector2I(2, 5));
    // 0 wall - 0 corner (1)
    AddBitmaskEntries(new () {true , true , true , true , true , true , true , true }, new Vector2I(0, 0));
    // 0 wall - 1 corner (4)
    AddBitmaskEntries(new () {false, true , true , true , true , true , true , true }, new Vector2I(7, 1));
    AddBitmaskEntries(new () {true , true , false, true , true , true , true , true }, new Vector2I(6, 1));
    AddBitmaskEntries(new () {true , true , true , true , true , false, true , true }, new Vector2I(7, 0));
    AddBitmaskEntries(new () {true , true , true , true , true , true , true , false}, new Vector2I(6, 0));
    // 0 wall - 2 corners - adjacent (4)
    AddBitmaskEntries(new () {false, true , false, true , true , true , true , true }, new Vector2I(6, 5));
    AddBitmaskEntries(new () {true , true , false, true , true , true , true , false}, new Vector2I(5, 4));
    AddBitmaskEntries(new () {true , true , true , true , true , false, true , false}, new Vector2I(6, 3));
    AddBitmaskEntries(new () {false, true , true , true , true , false, true , true }, new Vector2I(7, 4));
    // 0 wall - 2 corners - diagonal (2)
    AddBitmaskEntries(new () {false, true , true , true , true , true , true , false}, new Vector2I(7, 2));
    AddBitmaskEntries(new () {true , true , false, true , true , false, true , true }, new Vector2I(6, 2));
    // 0 wall - 3 corners (4)
    AddBitmaskEntries(new () {true , true , false, true , true , false, true , false}, new Vector2I(5, 3));
    AddBitmaskEntries(new () {false, true , true , true , true , false, true , false}, new Vector2I(7, 3));
    AddBitmaskEntries(new () {false, true , false, true , true , true , true , false}, new Vector2I(5, 5));
    AddBitmaskEntries(new () {false, true , false, true , true , false, true , true }, new Vector2I(7, 5));
    // 0 wall - 4 corners (1)
    AddBitmaskEntries(new () {false, true , false, true , true , false, true , false}, new Vector2I(6, 4));
  }
}
