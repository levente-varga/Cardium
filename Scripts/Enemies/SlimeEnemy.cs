using Godot;

namespace Cardium.Scripts.Enemies;

public partial class SlimeEnemy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        Texture = GD.Load<Texture2D>("res://assets/Sprites/Enemies/slime.png");
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
        base.OnDamaged(source, damage);
        
        GD.Print(Name + " received " + damage + " damage from " + source.Name + ". Current health: " + Health + "/" + MaxHealth);
    }

    public override void OnTargeted(Entity source)
    {
        
    }
    
    public override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}