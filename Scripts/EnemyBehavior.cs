using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public abstract class EnemyBehavior
{
    public abstract int MaxHealth { get; }
    public int Health { get; protected set;  }
    public abstract int MaxEnergy { get; }
    public int Energy { get; protected set; }
    public abstract int Armor { get; }
    public abstract int Damage { get; }
    public abstract int Range { get; }
    public abstract int Vision { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Sprite { get; }
    public List<Card> Inventory = new();
    
    public delegate void OnDeathDelegate(EnemyBehavior enemy);
    public event OnDeathDelegate OnDeathEvent;

    public abstract void OnTurn(Player player);

    public virtual void OnDamaged(Player player, int damage)
    {
        Health -= Math.Min(1, damage - Armor);
        if (Health < 0) OnDeath(player);
        
    }
    
    public abstract void OnTargeted(Player player);
    
    public virtual void OnDeath(Player player)
    {
        Health = 0;
        OnDeathEvent?.Invoke(this);
    }
}