using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Ranger : Enemy
{
    public override void _Ready()
    {
        base._Ready();

        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Ranger.png"), 8, 12);
        Name = "Ranger";
        Description = "Stays just in range.";
        MaxHealth = 1;
        Health = MaxHealth;
        BaseVision = 6;
        BaseArmor = 0;
        BaseDamage = 1;
        BaseRange = 4;
    }

    protected override void TakeTurn(Player player, World world)
    {
        base.TakeTurn(player, world);
    }
}