using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Ranger : Enemy
{
    private Label DebugLabel;
    
    public override void _Ready()
    {
        base._Ready();

        Name = "Ranger";
        Description = "Stays just in range.";
        MaxHealth = 1;
        Health = MaxHealth;
        MaxEnergy = 2;
        Energy = MaxEnergy;
        BaseArmor = 0;
        BaseDamage = 1;
        BaseRange = 4;
        BaseVision = 5;
        CombatVision = 7;
        
        DebugLabel = GetNode<Label>("/root/Root/Camera2D/CanvasLayer/Label4");
        
        SetStillFrame(GD.Load<Texture2D>("res://assets/Sprites/player.png"));
    }

    protected override async Task Turn(Player player, World world)
    {
        await Delay(300);
        
        var tilesExactlyInRange = world.GetEmptyTilesExactlyInRange(player.Position, BaseRange, exclude: Position);
        var tileDistances = tilesExactlyInRange.Select(tile => Position.DistanceTo(tile)).ToList();
        
        Path.SetPath(tilesExactlyInRange.Select(v => new Vector2(v.X, v.Y) * Global.TileSize.X + Global.TileSize / 2).ToArray());
        
        SpawnDebugFallingLabel("Found " + tilesExactlyInRange.Count + " empty tiles in range");
        await DebugDelay(700);
        SpawnDebugFallingLabel("Minimum distance: " + tileDistances.Min());
        await DebugDelay(700);

        DebugLabel.Text = "Position: " + Position + "\n" +
                          "Tiles in range: ";
        foreach (var tile in tilesExactlyInRange)
        {
            DebugLabel.Text += tile + " ";
        }
        
        // Move if not exactly in range
        if (tileDistances.Min() != 0)
        {
            var orderedTiles = new List<Vector2I>();
            
            SpawnDebugFallingLabel("Finding closest tile to move to");
            await DebugDelay(700);

            // TODO: improve sort from O(n^2)
            foreach (var t in tilesExactlyInRange)
            {
                var minDistance = tileDistances.Min();
                var index = tileDistances.IndexOf(minDistance);
                orderedTiles.Add(tilesExactlyInRange[index]);
                tileDistances[index] = int.MaxValue;
            }

            if (orderedTiles.Count == 0)
            {
                SpawnDebugFallingLabel("No tiles in orderedTiles list");
                await DebugDelay(700);
            }
        
            foreach (var tile in orderedTiles)
            {
                var path = world.GetPathBetween(Position, tile);
                if (path == null)
                {
                    SpawnDebugFallingLabel("Path to " + tile + " skipped, unreachable");
                    await DebugDelay(700);
                    continue;
                }
                else
                {
                    await DebugDelay(700);
                    SpawnDebugFallingLabel("Path accepted, " + path.Count + " tiles");
                    await DebugDelay(700);
                }

                Path.SetPath(world.GetPointPathBetween(Position, tile));
                
                for (var i = 0; i < path.Count && Energy > 0; i++)
                {
                    await Move(path[i], world);
                    await Delay(300);
                }
                
                break;
            }
        }
        
        if (Energy == 0)
        {
            OnTurnEnd();
            return;
        }
        
        SpawnDebugFloatingLabel(Energy + " energy left after moving");
        await DebugDelay(700);
        
        // Attack
        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            player.ReceiveDamage(this, BaseDamage);
            Nudge(VectorToDirection(player.Position - Position));
            Energy--;
            await Delay(300);
        }
    }
}