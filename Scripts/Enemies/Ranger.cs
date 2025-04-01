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
        BaseArmor = 0;
        BaseDamage = 1;
        BaseRange = 4;
        
        DebugLabel = GetNode<Label>("/root/Root/Camera2D/CanvasLayer/Label4");
        
        SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
    }

    protected override void TakeTurn(Player player, World world)
    {
        base.TakeTurn(player, world);
    }
}