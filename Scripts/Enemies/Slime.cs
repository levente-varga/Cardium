using System.Threading.Tasks;
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
        Vision = 2;
        CombatVision = 4;
        Range = 1;
        Description = "A slime enemy.";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override async Task OnTurn(Player player, World world)
    {
        base.OnTurn(player, world);
    }

    public override void ReceiveDamage(Entity source, int damage)
    {
        base.ReceiveDamage(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        
    }
    
    protected override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}