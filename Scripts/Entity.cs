using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Entity : TileAlignedGameObject
{
    public int Health { 
        get => HealthBar.Health;
        protected set => HealthBar.Health = value;
    }
    public int MaxHealth
    {
        get => HealthBar.MaxHealth;
        set => HealthBar.MaxHealth = value;
    }
    public int Energy { get; protected set; } = 1;
    public int MaxEnergy = 1;
    public int Armor;
    public int Damage;
    public float Luck;
    public float Vision;
    public float Range;
    public string Description;
    public bool InCombat;
    
    public List<Card> Inventory = new();
    
    public HealthBar HealthBar;
    
    public delegate void OnDeathDelegate(Entity entity);
    public event OnDeathDelegate OnDeathEvent;
    
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        base._Ready();
        
        HealthBar = new HealthBar();
        AddChild(HealthBar);
        Position = Vector2I.Zero;
        Name = "Entity";
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
    
    public virtual void OnTurn(Entity source)
    {
        
    }

    public virtual void OnDamaged(Entity source, int damage)
    {
        // TODO: Implement dodge based on luck
        Health -= Math.Min(1, damage - Armor);
        if (Health <= 0) OnDeath(source);
    }

    public virtual void OnTargeted(Entity source)
    {
        
    }
    
    public virtual void OnDeath(Entity source)
    {
        Health = 0;
        OnDeathEvent?.Invoke(this);
    }

    public bool InRange(Vector2I position)
    {
        return Position.DistanceTo(position) <= Range;
    }
    
    public bool InVision(Vector2I position)
    {
        return Position.DistanceTo(position) <= Vision;
    }
}