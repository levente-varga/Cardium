using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Ranger : Enemy
{
    public override void _Ready()
    {
        base._Ready();

        Name = "Ranger";
        Description = "Stays just in range.";
        MaxHealth = 1;
        Health = MaxHealth;
        MaxEnergy = 2;
        Energy = MaxEnergy;
        Armor = 0;
        Damage = 1;
        Range = 6;
        Vision = 7;
        CombatVision = 10;
        
        SetAnimation("idle", GD.Load<Texture2D>("res://assets/Animations/Ranger.png"), 8, 12);
    }

    public override void OnTurn(Player player, World world)
    {
        base.OnTurn(player, world);
        
        var tilesExactlyInRange = world.GetEmptyTilesExactlyInRange(player.Position, Range);
        var tileDistances = tilesExactlyInRange.Select(tile => Position.DistanceTo(tile)).ToList();
        
        // Move if not exactly in range
        if (tileDistances.Min() != 0)
        {
            var orderedTiles = new List<Vector2I>();

            // TODO: improve sort from O(n^2)
            foreach (var t in tilesExactlyInRange)
            {
                var minDistance = tileDistances.Min();
                var index = tileDistances.IndexOf(minDistance);
                orderedTiles.Add(tilesExactlyInRange[index]);
                tileDistances[index] = int.MaxValue;
            }
        
            foreach (var tile in orderedTiles)
            {
                var path = world.GetPathBetween(Position, tile);
                if (path == null)
                {
                    GD.Print("Path skipped, unreachable");
                    continue;
                }

                for (var i = 0; i < path.Count && Energy > 0; i++)
                {
                    Move(path[i], world);
                }
                
                break;
            }
        }
        
        // Attack
        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            player.ReceiveDamage(this, Damage);
        }
    }
}