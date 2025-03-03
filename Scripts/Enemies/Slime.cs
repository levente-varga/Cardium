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
        if (Energy > 0)
        {
            if (world.GetDistanceBetween(Position, player.Position) <= Range)
            {
                // TODO: Attack player
            }
            else
            {
                var path = world.GetPathBetween(Position, player.Position);
                if (path.Count > 0)
                {
                    Position = path[0];
                    Energy--;
                }
            }
        }
        
        base.OnTurn(player, world);
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