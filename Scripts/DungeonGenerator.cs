using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Enemies;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

public partial class Dungeon {
  private class Generator {
    public int NumRoomTries = 100;
    public static int ExtraConnectorChance => 5;
    public static int RoomExtraSize => 0;
    public static int WindingPercent => 70;
    public static int MinSmallRooms => 2;
    public static Vector2I MinSize => new(11, 11);

    private static readonly List<Vector2I> Directions = new() {
      Vector2I.Down, Vector2I.Up, Vector2I.Left, Vector2I.Right
    };

    private readonly Random _random = new();

    /// For each open position in the dungeon, the index of the connected region
    /// that that position is a part of.
    private readonly List<List<int>> _regions = new();

    /// The index of the current region being carved.
    private int _currentRegion = -1;

    private Dungeon _dungeon = new();

    public Dungeon GenerateDungeon(int width, int height, int roomTries = 100) =>
      GenerateDungeon(new Vector2I(width, height), roomTries);

    public Dungeon GenerateDungeon(Vector2I size, int roomTries = 100) {
      GD.Print($"Started generating a {size.X}x{size.Y} dungeon");

      if (size.X % 2 == 0 || size.Y % 2 == 0) {
        throw new Exception("The dungeon must be odd-sized.");
      }

      if (size.X <= MinSize.X || size.Y <= MinSize.Y) {
        throw new Exception($"The dungeon must be at least {MinSize.X}x{MinSize.Y} to ensure everything fits inside.");
      }

      _dungeon = new Dungeon {
        Rect = new Rect2I(Vector2I.Zero, size)
      };
      NumRoomTries = roomTries;

      InitArea();
      GenerateRooms();
      GenerateMaze();
      ConnectRegions();
      RemoveDeadEnds();

      CategorizeRooms();
      PlaceWalls();
      PlaceExits();
      PlaceBonfires();
      PlaceChests();
      PlaceDoors();
      PlaceEnemies();
      PlacePlayer();
      DecorateGround();

      GD.Print($"" +
               $"Generated a {_dungeon.Rect.Size.X}x{_dungeon.Rect.Size.Y} dungeon with\n" +
               $"  {_dungeon.Rooms.Count} rooms\n" +
               $"    {_dungeon.Rooms.Where(RoomIsSmall).ToList().Count} small\n" +
               $"  {_dungeon.Enemies.Count} enemies\n" +
               $"    {_dungeon.Enemies.Where(e => e is Slime).ToList().Count} slimes\n" +
               $"    {_dungeon.Enemies.Where(e => e is Spider).ToList().Count} spiders\n" +
               $"    {_dungeon.Enemies.Where(e => e is Ranger).ToList().Count} rangers\n" +
               $"    {_dungeon.Enemies.Where(e => e is Voidling).ToList().Count} voidlings\n" +
               $"    {_dungeon.Enemies.Where(e => e is Exterminator).ToList().Count} exterminators\n" +
               $"  {_dungeon.Interactables.Count} interactables\n" +
               $"    {_dungeon.Interactables.Where(i => i is Ladder).ToList().Count} exits\n" +
               $"    {_dungeon.Interactables.Where(i => i is Bonfire).ToList().Count} bonfires\n" +
               $"    {_dungeon.Interactables.Where(i => i is Chest).ToList().Count} chests\n" +
               $"    {_dungeon.Interactables.Where(i => i is Door).ToList().Count} doors\n");

      return _dungeon;
    }

    public Dungeon GenerateLobbyDungeon() {
      _dungeon = new Dungeon {
        Rect = new Rect2I(0, 0, 11, 7),
      };
      InitArea();

      CarveRect(new Rect2I(1, 2, 4, 3), TileTypes.RoomInterior);
      CarveRect(new Rect2I(5, 1, 5, 5), TileTypes.RoomInterior);
      Carve(10, 3, TileTypes.Doorway);

      PlaceWalls();

      _dungeon.Interactables.Add(new Entrance { Position = new Vector2I(10, 3) });
      _dungeon.Interactables.Add(new Bonfire { Position = new Vector2I(7, 3), Extinguishable = false });
      _dungeon.Interactables.Add(new Exit { Position = new Vector2I(2, 3) });
      _dungeon.Interactables.Add(new Stash { Position = new Vector2I(6, 1) });
      _dungeon.Interactables.Add(new Workbench { Position = new Vector2I(8, 1) });
      _dungeon.Interactables.Add(new Sign { Position = new Vector2I(12, 2), Text = "That way!" });
      if (Statistics.Deaths > 0) _dungeon.Interactables.Add(new Grave { Position = new Vector2I(7, 5) });
      if (Statistics.Runs > 0) _dungeon.Interactables.Add(new Records { Position = new Vector2I(4, 2) });
      _dungeon.Player.Position = new Vector2I(5, 3);

      DecorateGround();

      return _dungeon;
    }

    /// Implementation of the "growing tree" algorithm from here:
    /// http://www.astrolog.org/labyrnth/algrithm.htm.
    private void GrowMaze(Vector2I start) {
      var cells = new List<Vector2I>();
      var lastDirection = Vector2I.Zero;

      StartRegion();
      Carve(start);

      cells.Add(start);
      while (cells.Count > 0) {
        var cell = cells.Last();

        // See which adjacent cells are open.
        var unmadeCells = Directions.Where(direction => CanCarve(cell, direction)).ToList();

        if (unmadeCells.Count > 0) {
          // Based on how "windy" passages are, try to prefer carving in the
          // same direction.
          Vector2I direction;
          if (unmadeCells.Contains(lastDirection) && _random.Next(100) > WindingPercent) {
            direction = lastDirection;
          }
          else {
            direction = unmadeCells[_random.Next(0, unmadeCells.Count)];
          }

          Carve(cell + direction);
          Carve(cell + direction * 2);

          cells.Add(cell + direction * 2);
          lastDirection = direction;
        }
        else {
          cells.Remove(cell);
          lastDirection = Vector2I.Zero;
        }
      }
    }

    private void GenerateRooms() {
      // Generate the minimum required small (3x3) rooms, for a guaranteed exit and spawn point
      var remainingSmallRooms = MinSmallRooms;
      while (remainingSmallRooms > 0) {
        if (PlaceRoom(3, 3)) {
          remainingSmallRooms--;
        }
      }

      for (var i = 0; i < NumRoomTries; i++) {
        var size = _random.Next(1, 3 + RoomExtraSize) * 2 + 1;
        var rectangularity = _random.Next(0, 1 + size / 2) * 2;
        var width = size;
        var height = size;
        if (_random.Next(0, 2) == 0) {
          width += rectangularity;
        }
        else {
          height += rectangularity;
        }

        PlaceRoom(new Vector2I(width, height));
      }
    }

    private bool PlaceRoom(int width, int height) => PlaceRoom(new Vector2I(width, height));

    private bool PlaceRoom(Vector2I size) {
      Vector2I position = new(
        _random.Next(0, (_dungeon.Rect.Size.X - size.X) / 2) * 2 + 1,
        _random.Next(0, (_dungeon.Rect.Size.Y - size.Y) / 2) * 2 + 1
      );

      var room = new Room {
        Rect = new Rect2I(position.X, position.Y, size.X, size.Y),
        Type = RoomTypes.Uncategorized
      };

      var overlaps = _dungeon.Rooms.Any(other => room.Rect.Intersects(other.Rect));
      if (overlaps) return false;

      _dungeon.Rooms.Add(room);

      StartRegion();
      for (var x = room.Rect.Position.X; x < room.Rect.End.X; x++) {
        var perimeterX = x == room.Rect.Position.X || x == room.Rect.End.X - 1;
        for (var y = room.Rect.Position.Y; y < room.Rect.End.Y; y++) {
          var perimeterY = y == room.Rect.Position.Y || y == room.Rect.End.Y - 1;
          var type = TileTypes.RoomInterior;
          if (perimeterX && perimeterY) type = TileTypes.RoomCorner;
          else if (perimeterX || perimeterY) type = TileTypes.RoomPerimeter;
          Carve(new Vector2I(x, y), type);
        }
      }

      return true;
    }

    private void GenerateMaze() {
      for (var y = 1; y < _dungeon.Rect.Size.Y; y += 2) {
        for (var x = 1; x < _dungeon.Rect.Size.X; x += 2) {
          var tile = new Vector2I(x, y);
          if (!IsWall(tile)) continue;
          GrowMaze(tile);
        }
      }
    }

    private void ConnectRegions() {
      // Find all the tiles that can connect two (or more) regions.
      var connectorRegions = new Dictionary<Vector2I, HashSet<int>>();

      var shrunkenBounds = _dungeon.Rect.Grow(-1);

      for (var x = shrunkenBounds.Position.X; x < shrunkenBounds.End.X; x++) {
        for (var y = shrunkenBounds.Position.Y; y < shrunkenBounds.End.Y; y++) {
          var pos = new Vector2I(x, y);

          if (!IsWall(pos)) continue;

          var regions = new HashSet<int>();
          foreach (var dir in Directions) {
            var dirPos = pos + dir;
            if (IsWall(dirPos)) continue;
            var region = _regions[dirPos.X][dirPos.Y];
            regions.Add(region);
          }

          if (regions.Count < 2) continue;

          connectorRegions[pos] = regions;
        }
      }

      var connectors = connectorRegions.Keys.ToList();

      // Keep track of which regions have been merged. This maps an original
      // region index to the one it has been merged into.
      var mergedRegions = new List<int>();

      var openRegions = new List<int>();
      for (var i = 0; i <= _currentRegion; i++) {
        mergedRegions.Add(i);
        openRegions.Add(i);
      }

      // Keep connecting regions until we're down to one.
      while (openRegions.Count > 1) {
        var index = _random.Next(0, connectors.Count);
        var connector = connectors[index];

        // Carve the connection.
        AddJunction(connector);

        // Merge the connected regions. We'll pick one region (arbitrarily) and
        // map all the other regions to its index.
        var regions = connectorRegions[connector].Select(region => mergedRegions[region]).ToList();
        var target = regions.First();
        var sources = regions.Skip(1).ToList();

        // Merge all the affected regions. We have to look at all the
        // regions because other regions may have previously been merged with
        // some of the ones we're merging now.
        for (var i = 0; i <= _currentRegion; i++) {
          if (sources.Contains(mergedRegions[i])) {
            mergedRegions[i] = target;
          }
        }

        // The sources are no longer in use.
        openRegions = openRegions.Except(sources).ToList();

        // Remove any connectors that aren't needed anymore.
        connectors.RemoveAll(pos => {
          // Don't allow connectors right next to each other.
          if (connector.DistanceTo(pos) < 2) return true;

          // If the connector no longer spans different regions, we don't need it.
          HashSet<int> adjacentRegions = new();
          foreach (var region in connectorRegions[pos]) {
            adjacentRegions.Add(mergedRegions[region]);
          }

          if (adjacentRegions.Count > 1) return false;

          if (_random.Next(ExtraConnectorChance) == 0) AddJunction(pos);

          return true;
        });
      }
    }

    private void AddJunction(Vector2I tile) {
      for (var x = -1; x <= 1; x++) {
        for (var y = -1; y <= 1; y++) {
          if (Mathf.Abs(x + y) != 1) continue;
          if (_dungeon.Tiles[tile.X + x][tile.Y + y] == TileTypes.RoomPerimeter ||
              _dungeon.Tiles[tile.X + x][tile.Y + y] == TileTypes.RoomCorner) {
            SetTile(new Vector2I(tile.X + x, tile.Y + y), TileTypes.Doorway);
          }
        }
      }

      SetTile(tile, TileTypes.Entrance);
    }

    private void RemoveDeadEnds() {
      var done = false;

      while (!done) {
        done = true;

        var shrunkenBounds = _dungeon.Rect.Grow(-1);
        for (var x = shrunkenBounds.Position.X; x < shrunkenBounds.End.X; x++) {
          for (var y = shrunkenBounds.Position.Y; y < shrunkenBounds.End.Y; y++) {
            var pos = new Vector2I(x, y);

            if (IsWall(pos)) continue;

            // If it only has one exit, it's a dead end.
            var exits = Directions.Count(dir => !IsWall(pos + dir));
            if (exits != 1) continue;

            done = false;
            SetTile(pos);
          }
        }
      }
    }

    /// Gets whether an opening can be carved from the given starting
    /// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
    /// if the starting Cell is in bounds and the destination Cell is filled
    /// (or out of bounds).
    private bool CanCarve(Vector2I pos, Vector2I direction) {
      // Must end in bounds.
      if (!new Rect2I(Vector2I.Zero, _dungeon.Rect.Size).HasPoint(pos + direction * 3)) return false;

      // Destination must not be open.
      return IsWall(pos + direction * 2);
    }

    private void StartRegion() {
      _currentRegion++;
    }

    private void CarveRect(Rect2I rect, TileTypes type = TileTypes.Corridor) {
      for (var x = rect.Position.X; x < rect.End.X; x++) {
        for (var y = rect.Position.Y; y < rect.End.Y; y++) {
          Carve(new Vector2I(x, y), type);
        }
      }
    }

    private void CarveRoom(Room room) {
      for (var x = room.Rect.Position.X; x < room.Rect.End.X; x++) {
        var perimeterX = x == room.Rect.Position.X || x == room.Rect.End.X - 1;
        for (var y = room.Rect.Position.Y; y < room.Rect.End.Y; y++) {
          var perimeterY = y == room.Rect.Position.Y || y == room.Rect.End.Y - 1;
          var type = TileTypes.RoomInterior;
          if (perimeterX && perimeterY) type = TileTypes.RoomCorner;
          else if (perimeterX || perimeterY) type = TileTypes.RoomPerimeter;
          Carve(new Vector2I(x, y), type);
        }
      }
    }

    private void Carve(int x, int y, TileTypes type = TileTypes.Corridor) => Carve(new Vector2I(x, y), type);

    private void Carve(Vector2I tile, TileTypes type = TileTypes.Corridor) {
      SetTile(tile, type);
      _regions[tile.X][tile.Y] = _currentRegion;
    }

    private bool IsWall(Vector2I tile) => _dungeon.Tiles[tile.X][tile.Y] == TileTypes.Wall;
    private void SetTile(Vector2I tile, TileTypes value = TileTypes.Wall) => SetTile(tile.X, tile.Y, value);
    private void SetTile(int x, int y, TileTypes value = TileTypes.Wall) => _dungeon.Tiles[x][y] = value;

    private void InitArea() {
      for (var x = 0; x < _dungeon.Rect.Size.X; x++) {
        _dungeon.Tiles.Add(new List<TileTypes>());
        _regions.Add(new List<int>());
        for (var y = 0; y < _dungeon.Rect.Size.Y; y++) {
          _dungeon.Tiles[x].Add(TileTypes.Wall);
          _regions[x].Add(-1);
        }
      }
    }

    private static bool RoomIsSmall(Room room) => room.Rect.Area <= 15;

    private void CategorizeRooms() {
      var spawnRoomPicked = false;
      var bonfires = 0;
      var exits = 0;
      foreach (var room in _dungeon.Rooms) {
        if (room.Type != RoomTypes.Uncategorized) continue;
        if (RoomIsSmall(room)) {
          if (!spawnRoomPicked) {
            room.Type = RoomTypes.Spawn;
            spawnRoomPicked = true;
          }
          else {
            if (_random.Next((int)Mathf.Pow(exits * 2f, 2)) == 0) {
              exits++;
              room.Type = RoomTypes.Exit;
            }
            else if (_random.Next((int)Mathf.Pow(bonfires, 1.2f)) == 0) {
              bonfires++;
              room.Type = RoomTypes.Bonfire;
            }
          }
        }
      }
    }

    private static bool LayerIsEmptyAt(TileMapLayer layer, Vector2I tile) => layer.GetCellSourceId(tile) == -1;

    /// <summary>
    /// Sets wall sprites according to their surrounding cells
    /// </summary>
    private void PlaceWalls() {
      for (var x = 0; x < _dungeon.Rect.Size.X; x++) {
        for (var y = 0; y < _dungeon.Rect.Size.Y; y++) {
          if (_dungeon.Tiles[x][y] != TileTypes.Wall) continue;
          var bitmask = _dungeon.GetWallBitmask(x, y);
          var atlasCoords = _dungeon._bitmaskToWallAtlasCoord[bitmask];
          _dungeon.WallLayer.SetCell(new Vector2I(x, y), 0, atlasCoords);
        }
      }
    }

    private void DecorateGround() {
      var occupiedTiles = new HashSet<Vector2I>();
      _dungeon.Interactables.ForEach(e => occupiedTiles.Add(e.Position));
      _dungeon.Enemies.ForEach(e => occupiedTiles.Add(e.Position));
      for (var x = 0; x < _dungeon.Rect.Size.X; x++) {
        for (var y = 0; y < _dungeon.Rect.Size.Y; y++) {
          if (!LayerIsEmptyAt(_dungeon.WallLayer, new Vector2I(x, y))) continue;
          if (occupiedTiles.Contains(new Vector2I(x, y))) continue;

          var factor = _dungeon.Tiles[x][y] switch {
            TileTypes.RoomCorner => 0.4f,
            TileTypes.RoomPerimeter => 0.8f,
            TileTypes.RoomInterior => 1.2f,
            _ => 1
          };

          var random = _random.Next(100);
          random = (int)(random * factor);

          switch (random) {
            case 0: _dungeon.DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(8, 0)); break;
            case <= 2: _dungeon.DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(5, 0)); break;
            case <= 6: _dungeon.DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(2, 0)); break;
            case <= 18: _dungeon.DecorLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0)); break;
          }
        }
      }
    }

    private void PlaceEnemies() {
      bool bossPlaced = false;
      foreach (var room in _dungeon.Rooms.Where(room => room.Type == RoomTypes.Uncategorized)) {
        var enemyCount = _random.Next(room.Rect.Grow(-1).Area / 4) + 1;
        var usedTiles = new List<Vector2I>();

        if (!bossPlaced) enemyCount = 1;

        for (var i = 0; i < enemyCount; i++) {
          Vector2I tile;
          do {
            tile = new Vector2I(
              _random.Next(room.Rect.Position.X + 1, room.Rect.End.X - 2),
              _random.Next(room.Rect.Position.Y + 1, room.Rect.End.Y - 2)
            );
          } while (usedTiles.Contains(tile));

          Enemy enemy;
          var level = _random.Next(100) switch {
            < 50 => 0,
            < 75 => 1,
            < 90 => 2,
            < 98 => 3,
            _ => 4
          };

          if (!bossPlaced) {
            enemy = new Exterminator();
            bossPlaced = true;
          }
          else {
            enemy = _random.Next(100) switch {
              < 50 => new Slime { Level = level },
              < 80 => new Spider { Level = level },
              < 90 => new Ranger { Level = level },
              _ => new Voidling { Level = level },
            };
          }

          enemy.Position = tile;
          _dungeon.Enemies.Add(enemy);
          usedTiles.Add(tile);
        }
      }
    }

    private void PlaceDoors() {
      var doors = 0;
      for (var x = 0; x < _dungeon.Rect.Size.X; x++) {
        for (var y = 0; y < _dungeon.Rect.Size.Y; y++) {
          if (_dungeon.Tiles[x][y] != TileTypes.Entrance) continue;
          if (_random.Next(doors + 2) > 0) continue;
          Door door = new() { Position = new Vector2I(x, y) };
          _dungeon.Interactables.Add(door);
          doors++;
        }
      }
    }

    private void PlaceBonfires() {
      foreach (var room in _dungeon.Rooms.Where(room => room.Type == RoomTypes.Bonfire)) {
        var tileCandidates = GetRoomInteriorTiles(room);

        Bonfire bonfire = new();
        bonfire.Position = tileCandidates[_random.Next(tileCandidates.Count)];
        _dungeon.Interactables.Add(bonfire);
      }
    }

    private void PlaceChests() {
      var chests = 0;
      foreach (var room in _dungeon.Rooms.Where(room => room.Type == RoomTypes.Uncategorized)) {
        if (_random.Next(chests / 2) > 0) continue;
        var tileCandidates = GetRoomPerimeterTiles(room);
        if (tileCandidates.Count == 0) continue;
        Chest chest = new();
        chest.Position = tileCandidates[_random.Next(tileCandidates.Count)];
        _dungeon.Interactables.Add(chest);
        chests++;
      }
    }

    private void PlaceExits() {
      foreach (var room in _dungeon.Rooms.Where(room => room.Type == RoomTypes.Exit)) {
        var tileCandidates = GetRoomInteriorTiles(room);

        Ladder ladder = new();
        ladder.Position = tileCandidates[_random.Next(tileCandidates.Count)];
        _dungeon.Interactables.Add(ladder);
      }
    }

    private void PlacePlayer() {
      if (_dungeon.Rooms.Where(room => room.Type == RoomTypes.Spawn).ToList().Count == 0) {
        GD.Print("[ERROR] No spawn room was generated");
        return;
      }

      var room = _dungeon.Rooms.FirstOrDefault(room => room.Type == RoomTypes.Spawn);
      if (room == null) return;
      var tileCandidates = GetRoomInteriorTiles(room);

      _dungeon.Player.Position = tileCandidates[_random.Next(tileCandidates.Count)];
    }

    private List<Vector2I> GetRoomInteriorTiles(Room room) {
      List<Vector2I> tiles = new();
      for (var x = room.Rect.Position.X; x < room.Rect.End.X; x++) {
        for (var y = room.Rect.Position.Y; y < room.Rect.End.Y; y++) {
          if (_dungeon.Tiles[x][y] == TileTypes.RoomInterior) tiles.Add(new Vector2I(x, y));
        }
      }

      return tiles;
    }

    private List<Vector2I> GetRoomPerimeterTiles(Room room) {
      List<Vector2I> tiles = new();
      for (var x = room.Rect.Position.X; x < room.Rect.End.X; x++) {
        for (var y = room.Rect.Position.Y; y < room.Rect.End.Y; y++) {
          if (_dungeon.Tiles[x][y] == TileTypes.RoomPerimeter) tiles.Add(new Vector2I(x, y));
        }
      }

      return tiles;
    }
  }
}