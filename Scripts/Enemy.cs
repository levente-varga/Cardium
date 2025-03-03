using System.Linq;
using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    private Path _path;
    
    
    public override void _Ready()
    {
        base._Ready();
        _path = new Path();
        AddChild(_path);
        
        HealthBar.Visible = false;
    }
    
    public override void _Process(double delta)
    {
        _path.Visible = InCombat;
        base._Process(delta);
    }

    public override void OnTurn(Player player, World world)
    {
        base.OnTurn(player, world);
        
        _path.SetPath(world.GetPointPathBetween(Position, player.Position));
        
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public override void ReceiveDamage(Entity source, int damage)
    {
        Blink();
        
        GD.Print(Name + " received " + damage + " damage from " + source.Name + ". Current health: " + Health + "/" + MaxHealth);
        
        base.ReceiveDamage(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        base.OnTargeted(source);
    }
    
    protected override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }

    public void OnPlayerMove(TileAlignedGameObject entity, Vector2I oldPosition, Vector2I newPosition)
    {
        var player = entity as Player;
        if (InVision(newPosition))
        {
            if (InCombat) return;
            SetInCombatStatus(true);
            SpawnFloatingLabel("Spotted!", color: Global.Red, lifetimeMillis: 2000);
            player?.OnCombatStart();
        }
        else if (InCombat && !InCombatVision(newPosition))
        {
            SetInCombatStatus(false);
        }
    }
}