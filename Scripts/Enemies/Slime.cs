using System.Threading.Tasks;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Slime : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Slime.png"), 8, 12);
        Name = "Slime";
        MaxHealth = 3;
        Health = MaxHealth;
        MaxEnergy = 1;
        Energy = MaxEnergy;
        BaseArmor = 0;
        BaseDamage = 1;
        BaseLuck = 0f;
        BaseVision = 2;
        CombatVision = 4;
        BaseRange = 1;
        Description = "A slime enemy.";
        
        Inventory.Add(new HealCard());
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    protected override async Task Turn(Player player, World world)
    {
        await base.Turn(player, world);
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