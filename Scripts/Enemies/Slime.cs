using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Slime : Enemy
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
        CombatVision = 4f;
        Range = 1f;
        Description = "A slime enemy.";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Player player, World world)
    {
        SpawnFloatingLabel("[Debug] Start of turn", fontSize: 20);
        
        Energy = MaxEnergy;

        for (var i = 0; i < MaxEnergy && Energy > 0; i++)
        {
            var distance = world.GetDistanceBetween(Position, player.Position);
            if (distance == -1)
            {
                SpawnFloatingLabel("[Debug] Unreachable");
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
                    SpawnFloatingLabel("[Debug] Unable to move", color: Global.Magenta);
                }
            }
        }
        
        base.OnTurn(player, world);
        
        OnTurnFinished();
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