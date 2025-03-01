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

    public override void OnTurn(Entity source)
    {
        base.OnTurn(source);
        
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public override void OnDamaged(Entity source, int damage)
    {
        base.OnDamaged(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        base.OnTargeted(source);
    }
    
    public override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}