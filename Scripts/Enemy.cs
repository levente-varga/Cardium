using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    protected Path Path;

    public delegate void OnDeathDelegate(Enemy enemy);
    public event OnDeathDelegate OnDeathEvent;
    
    public delegate void OnDamaged(Enemy enemy);
    public event OnDamaged OnDamagedEvent;
    
    public override void _Ready()
    {
        base._Ready();
        Path = new Path();
        AddChild(Path);
        
        HealthBar.Visible = false;
    }
    
    public override void _Process(double delta)
    {
        Path.Visible = true;
        base._Process(delta);
    }

    protected override void TakeTurn(Player player, World world)
    {
        Path.SetPath(world.GetPointPathBetween(Position, player.Position));
        
        if (Global.Debug) SpawnFloatingLabel("[Debug] Start of turn", color: Global.Magenta, fontSize: 20);

        var distance = world.GetDistanceBetween(Position, player.Position);
        
        if (distance == -1)
        {
            if (Global.Debug) SpawnFloatingLabel("[Debug] Unreachable", color: Global.Magenta, fontSize: 20);
            return;
        } 
        
        if (distance <= BaseRange)
        {
            Nudge(VectorToDirection(player.Position - Position));
            player.ReceiveDamage(this, BaseDamage);
            return;
        }
        
        var path = world.GetPathBetween(Position, player.Position);
        if (path is { Count: > 1 })
        {
            Move(path[0], world);
        }
        else
        {
            if (Global.Debug) SpawnFloatingLabel("[Debug] Unable to move", color: Global.Magenta, fontSize: 20);
        }
        
        base.TakeTurn(player, world);
    }

    public override void ReceiveDamage(Entity source, int damage)
    {
        GD.Print(Name + " received " + damage + " damage from " + source.Name + ". Current health: " + Health + "/" + MaxHealth);
        
        OnDamagedEvent?.Invoke(this);
        
        base.ReceiveDamage(source, damage);
    }
    
    protected override void OnDeath(Entity source)
    {
        base.OnDeath(source);
        
        OnDeathEvent?.Invoke(this);
    }

    public void OnPlayerMove(Player player, Vector2I oldPosition, Vector2I newPosition, CombatManager manager)
    {
        
    }
}