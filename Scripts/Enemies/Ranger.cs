using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        MaxEnergy = 1;
        Energy = MaxEnergy;
        Armor = 0;
        Damage = 1;
        Range = 4;
        Vision = 5;
        CombatVision = 7;
        
        SetStillFrame(GD.Load<Texture2D>("res://assets/Sprites/player.png"));
    }

    public override async Task OnTurn(Player player, World world)
    {
        TurnMarker.Visible = true;
        
        var tilesExactlyInRange = world.GetEmptyTilesExactlyInRange(player.Position, Range);
        var tileDistances = tilesExactlyInRange.Select(tile => Position.DistanceTo(tile)).ToList();
        
        SpawnDebugFloatingLabel("Found " + tilesExactlyInRange.Count + " empty tiles in range");
        await WaitFor(0.3f);
        
        SpawnDebugFloatingLabel("Minimum distance: " + tileDistances.Min());
        await WaitFor(0.3f);
        
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

                Path.SetPath(world.GetPointPathBetween(Position, tile));
                
                for (var i = 0; i < path.Count && Energy > 0; i++)
                {
                    await Move(path[i], world);
                }
                
                break;
            }
        }
        
        SpawnDebugFloatingLabel("In range, attacking with " + Energy + " energy left");
        
        // Attack
        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            player.ReceiveDamage(this, Damage);
            Energy--;
        }
        
        OnTurnFinished();
    }
}