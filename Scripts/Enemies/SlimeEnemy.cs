using Godot;

namespace Cardium.Scripts.Enemies;

public partial class SlimeEnemy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        Texture = GD.Load<Texture2D>("res://assets/Sprites/Enemies/slime.png");
        Name = "Slime";
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
        GD.Print(Name + " received " + damage + " damage from " + source.Name);
        
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