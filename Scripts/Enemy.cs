using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    public HealthBar HealthBar;

    public override void _Ready()
    {
        base._Ready();
        
        SetupHealthBar();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
    private void SetupHealthBar()
    {
        HealthBar = GetNode<HealthBar>("HealthBar");
        if (HealthBar == null) return;
        HealthBar.MaxHealth = MaxHealth;
        HealthBar.Health = Health;
    }

    public virtual void OnTurn(Player player)
    {
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public virtual void OnDamaged(Player player, int damage)
    {
        base.OnDamaged(player, damage);
    }

    public virtual void OnTargeted(Player player)
    {
        
    }
    
    public virtual void OnDeath(Player player)
    {
        base.OnDeath(player);
    }
}