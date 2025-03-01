using System;
using Godot;

namespace Cardium.Scripts;

public partial class Entity : Sprite2D
{
    public int Health { get; protected set; } = 5;
    public int Energy { get; protected set; } = 5;

    public int MaxHealth;
    public int MaxEnergy;
    public int Armor;
    public int Damage;
    public float Luck;
    public float Vision;
    public float Range;
    public string Description;
    
    public delegate void OnMoveDelegate();
    public event OnMoveDelegate OnMoveEvent;
    
    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeathEvent;
    
    public new Vector2I Position { get; set; }
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        Position = Vector2I.Zero;
        Scale = new Vector2(Global.Scale, Global.Scale);
        Centered = false;
        Name = "Entity";
    }

    public override void _Process(double delta)
    {
        base.Position = base.Position.Lerp(Position * Global.TileSize, 0.1f);
        
        if (_previousPosition != Position)
        {
            OnMoveEvent?.Invoke();
            _previousPosition = Position;
        }
    }

    public void SetPosition(Vector2I position)
    {
        Position = position;
        base.Position = position * Global.TileSize;
    }
    
    public virtual void OnTurn(Entity source)
    {
        
    }

    public virtual void OnDamaged(Entity source, int damage)
    {
        // TODO: Implement dodge based on luck
        Health -= Math.Min(1, damage - Armor);
        if (Health < 0) OnDeath(source);
    }

    public virtual void OnTargeted(Entity source)
    {
        
    }
    
    public virtual void OnDeath(Entity source)
    {
        Health = 0;
        OnDeathEvent?.Invoke();
    }
}