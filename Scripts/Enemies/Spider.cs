using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Spider : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        SetAnimation("idle", GD.Load<Texture2D>("res://assets/Animations/Spider.png"), 8, 12);
        Name = "Spider";
        MaxHealth = 5;
        Health = MaxHealth;
        MaxEnergy = 2;
        Energy = MaxEnergy;
        Armor = 0;
        Damage = 2;
        Luck = 0f;
        Vision = 4;
        Range = 1;
        Description = "A spider enemy.";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Player player, World world)
    {
        OnTurnFinished();
        
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
        
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