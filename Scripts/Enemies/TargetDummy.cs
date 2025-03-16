using System.Threading.Tasks;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class TargetDummy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        Name = "Target Dummy";
        MaxHealth = 1000;
        Health = MaxHealth;
        MaxEnergy = 0;
        Energy = MaxEnergy;
        BaseArmor = 0;
        BaseVision = 1;
        CombatVision = 2;
        SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
    }

    protected override Task Turn(Player player, World world)
    {
        Heal(MaxHealth);
        return Task.CompletedTask;
    }
}