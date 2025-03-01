using Godot;

namespace Cardium.Scripts.Enemies;

public partial class SlimeEnemy : Enemy
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
        Vision = 2f;
        Range = 1f;
        Description = "A slime enemy.";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Entity source)
    {
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public override void OnDamaged(Entity source, int damage)
    {
        Tween tween = GetTree().CreateTween();
        Modulate = new Color(0.25f, 0.25f, 0.25f, 1); // Set red
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
        
        GD.Print(Name + " received " + damage + " damage from " + source.Name + ". Current health: " + Health + "/" + MaxHealth);
        
        base.OnDamaged(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        
    }
    
    public override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}