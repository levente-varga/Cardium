using Godot;

namespace Cardium.Scripts;

public abstract class EnemyBehavior
{
    public abstract int MaxHealth { get; }
    public abstract int Health { get; }
    public abstract int MaxEnergy { get; }
    public abstract int Energy { get; }
    public abstract int Armor { get; }
    public abstract int Damage { get; }
    public abstract int Range { get; }
    public abstract int Vision { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Sprite { get; }

    public abstract void OnTurn(Player player);
    public abstract void OnDamaged(Player player, int damage);
    public abstract void OnTargeted(Player player);
}