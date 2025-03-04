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
        
        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            var distance = world.GetDistanceBetween(Position, player.Position);
            if (distance == Range)
            {
                // TODO: Attack player
            }
            else if (distance > Range)
            {
                // TODO: Move towards player until in Range
            }
            else
            {
                // TODO: Move away from player while in Range
            }
        }
    }
}