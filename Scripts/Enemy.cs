using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    public EnemyBehavior Behavior;
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}