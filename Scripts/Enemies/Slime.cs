using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Slime : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        SetAnimation("idle", GD.Load<Texture2D>("res://assets/Animations/Slime.png"), 8, 12);
        Name = "Slime";
        MaxHealth = 3;
        Health = MaxHealth;
        MaxEnergy = 1;
        Energy = MaxEnergy;
        Armor = 0;
        Damage = 1;
        Luck = 0f;
        Vision = 2f;
        Range = 1f;
        Description = "A slime enemy.";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Player player, World world)
    {
        GD.Print("Slime OnTurn called");
        Energy = MaxEnergy;

        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            if (world.GetDistanceBetween(Position, player.Position) <= Range)
            {
                // TODO: Attack player
            }
            else
            {
                var path = world.GetPathBetween(Position, player.Position);
                // print the whole path:
                GD.Print("Path: (from position " + Position + " to position " + player.Position + ")");
                foreach (var p in path)
                {
                    GD.Print(p);
                }
                if (path.Count > Range)
                {
                    Position = path[0];
                    Energy--;
                    SpawnFallingLabel("Path length: " + path.Count);
                }
                else
                {
                    player.OnDamaged(this, Damage);
                }
            }
        }
        
        base.OnTurn(player, world);
        
        OnTurnFinished();
    }

    public override void OnDamaged(Entity source, int damage)
    {
        base.OnDamaged(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        
    }
    
    public override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}