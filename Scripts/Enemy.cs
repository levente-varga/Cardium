using System.Linq;
using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    protected Path Path;
    
    
    public override void _Ready()
    {
        base._Ready();
        Path = new Path();
        AddChild(Path);
        
        HealthBar.Visible = false;
    }
    
    public override void _Process(double delta)
    {
        Path.Visible = InCombat;
        base._Process(delta);
    }

    public override void OnTurn(Player player, World world)
    {
        Path.SetPath(world.GetPointPathBetween(Position, player.Position));
        
        if (Global.Debug) SpawnFloatingLabel("[Debug] Start of turn", color: Global.Magenta, fontSize: 20);
        
        Energy = MaxEnergy;

        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            var distance = world.GetDistanceBetween(Position, player.Position);
            if (distance == -1)
            {
                if (Global.Debug) SpawnFloatingLabel("[Debug] Unreachable", color: Global.Magenta, fontSize: 20);
                OnTurnFinished();
                return;
            } 
            if (distance <= Range)
            {
                Nudge(VectorToDirection(player.Position - Position));
                player.ReceiveDamage(this, Damage);
            }
            else
            {
                var path = world.GetPathBetween(Position, player.Position);
                if (path.Count > 1)
                {
                    Move(path[0], world);
                }
                else
                {
                    if (Global.Debug) SpawnFloatingLabel("[Debug] Unable to move", color: Global.Magenta, fontSize: 20);
                }
            }
        }
        
        base.OnTurn(player, world);
        
        OnTurnFinished();
    }

    public override void ReceiveDamage(Entity source, int damage)
    {
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