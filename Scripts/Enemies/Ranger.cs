using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Ranger : Enemy {
  protected override int MaxLevel => 4;

  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Ranger.png"), 8, 12);
    Name = "Ranger";
    Description = "Stays just in range.";
    MaxHealth = new List<int> { 5, 7, 9, 12, 15 }[Level];
    Health = MaxHealth;
    BaseVision = 5;
    BaseCombatVision = 7;
    BaseArmor = new List<int> { 0, 0, 1, 1, 2 }[Level];
    BaseDamage = new List<int> { 1, 2, 2, 3, 3 }[Level];
    BaseRange = new List<int> { 3, 3, 3, 3, 3 }[Level];
  }

  protected override void TakeTurn(Player player, World world) {
    UpdateValue(player, world);
    if (!PlayerInVision) return;

    var tilesExactlyInRange = world.GetEmptyTilesExactlyInRange(player.Position, BaseRange, exclude: Position);
    var tileDistances = tilesExactlyInRange.Select(tile => Utils.ManhattanDistanceBetween(Position, tile)).ToList();

    // Move if not exactly in range
    if (tileDistances.Min() != 0) {
      var orderedTiles = new List<Vector2I>();

      // TODO: improve sort from O(n^2)
      foreach (var t in tilesExactlyInRange) {
        var minDistance = tileDistances.Min();
        var index = tileDistances.IndexOf(minDistance);
        orderedTiles.Add(tilesExactlyInRange[index]);
        tileDistances[index] = int.MaxValue;
      }

      foreach (var tile in orderedTiles) {
        var path = world.GetPathBetween(Position, tile);
        if (path == null || path.Count == 0) continue;

        Move(path[0], world);
        break;
      }
    }
    else {
      player.ReceiveDamage(this, BaseDamage, world);
      Nudge(VectorToDirection(player.Position - Position));
    }
  }

  protected override List<Card> GenerateLoot() {
    List<Card> loot = new();
    Random random = new();
      
    var indexCount = random.Next(1, 2 + Level);
    for (var i = 0; i < indexCount; i++) {
      loot.Add(
        random.Next(135) switch {
          < 40 => new SmiteCard(),
          < 80 => new ScoutCard(),
          < 100 => new ChainCard(),
          < 120 => new TeleportCard(),
          < 130 => new WoodenKeyCard(),
          < 135 => new GoldenKeyCard(),
          _ => new HealCard(),
        }
      );
    }

    return loot;
  }
}