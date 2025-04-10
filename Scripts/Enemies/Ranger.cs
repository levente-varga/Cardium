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
        BaseVision = 6;
        BaseArmor = 0;
        BaseDamage = 1;
        BaseRange = 4;
        
        SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
    }

    protected override void TakeTurn(Player player, World world)
    {
        base.TakeTurn(player, world);
    }
}