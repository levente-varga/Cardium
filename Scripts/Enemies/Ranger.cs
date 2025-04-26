using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Ranger : Enemy {
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Ranger.png"), 8, 12);
    Name = "Ranger";
    Description = "Stays just in range.";
    MaxHealth = 5;
    Health = MaxHealth;
    BaseVision = 5;
    BaseCombatVision = 7;
    BaseArmor = 0;
    BaseDamage = 1;
    BaseRange = 3;
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
      player.ReceiveDamage(this, BaseDamage);
      Nudge(VectorToDirection(player.Position - Position));
    }
  }
}