using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Enemies;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

public enum TileTypes {
  Wall,
  RoomInterior,
  RoomPerimeter,
  RoomCorner,
  Doorway,
  Entrance,
  Corridor,
}

public enum RoomTypes {
  Uncategorized,
  Spawn,
  Exit,
  Bonfire,
}

public class Room {
  public Rect2I Rect;
  public RoomTypes Type;
}

public partial class Dungeon {
  public readonly TileMapLayer GroundLayer = new();
  public readonly TileMapLayer DecorLayer = new();
  public readonly TileMapLayer WallLayer = new();
  public readonly TileMapLayer FogLayer = new();
  
  public Rect2I Rect { get; private set; }

  public readonly Player Player = new();
  public readonly List<Enemy> Enemies = new();
  public readonly List<Interactable> Interactables = new();
  public readonly List<List<TileTypes>> Tiles = new ();
  public readonly List<Room> Rooms = new();

  private readonly Dictionary<int, Vector2I> _bitmaskToWallAtlasCoord = new();
  
  public Dungeon() {
    FillBitmaskDictionary();
  }

  public static Dungeon Generate(int width, int height, int roomTries) => Generate(new Vector2I(width, height), roomTries);
  public static Dungeon Generate(Vector2I size, int roomTries) => new Generator().Generate(size, roomTries);

  /// <summary>
  /// Adds key-value pairs to the [_bitmaskToWallAtlasCoord] dictionary.
  /// </summary>
  /// <param name="pattern">
  /// Defines the rules for the generated bitmasks. Each list item corresponds to
  /// one bit in the bitmask (the first item is the largest bit). If a value is true, its bit
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
  
  private bool IsWall(Vector2I cell) => !Rect.HasPoint(cell) || Tiles[cell.X][cell.Y] == TileTypes.Wall;
  
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
