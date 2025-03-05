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
        MaxEnergy = 1;
        Energy = MaxEnergy;
        Armor = 0;
        Damage = 1;
        Range = 4;
        Vision = 5;
        CombatVision = 7;
        
        DebugLabel = GetNode<Label>("/root/Root/Camera2D/CanvasLayer/Label4");
        
        SetStillFrame(GD.Load<Texture2D>("res://assets/Sprites/player.png"));
    }

    public override async Task OnTurn(Player player, World world)
    {
        TurnMarker.Visible = true;

        Energy = MaxEnergy;
        
        var tilesExactlyInRange = world.GetEmptyTilesExactlyInRange(player.Position, Range, exclude: Position);
        var tileDistances = tilesExactlyInRange.Select(tile => Position.DistanceTo(tile)).ToList();
        
        Path.SetPath(tilesExactlyInRange.Select(v => new Vector2(v.X, v.Y) * Global.TileSize.X + Global.TileSize / 2).ToArray());
        
        SpawnDebugFallingLabel("Found " + tilesExactlyInRange.Count + " empty tiles in range");
        await WaitFor(0.7f);
        
        SpawnDebugFallingLabel("Minimum distance: " + tileDistances.Min());
        await WaitFor(0.7f);

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
            await WaitFor(0.7f);

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
                await WaitFor(0.7f);
            }
        
            foreach (var tile in orderedTiles)
            {
                var path = world.GetPathBetween(Position, tile);
                if (path == null)
                {
                    SpawnDebugFallingLabel("Path to " + tile + " skipped, unreachable");
                    await WaitFor(0.15f);
                    continue;
                }
                else
                {
                    await WaitFor(0.3f);
                    SpawnDebugFallingLabel("Path accepted, " + path.Count + " tiles");
                    await WaitFor(0.7f);
                }

                Path.SetPath(world.GetPointPathBetween(Position, tile));
                
                for (var i = 0; i < path.Count && Energy > 0; i++)
                {
                    await Move(path[i], world);
                    
                }
                
                break;
            }
        }
        
        if (Energy == 0)
        {
            OnTurnFinished();
            return;
        }
        
        SpawnDebugFloatingLabel(Energy + " energy left after moving");
        await WaitFor(0.7f);
        
        // Attack
        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            player.ReceiveDamage(this, Damage);
            Energy--;
            SpawnDebugFallingLabel("Attacked player");
            await WaitFor(0.7f);
        }
        
        OnTurnFinished();
    }
}