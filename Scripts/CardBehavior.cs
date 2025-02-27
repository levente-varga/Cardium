namespace Cardium.Scripts;

public enum CardType
{
    Combat,
    Action,
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public abstract class CardBehavior
{
    public abstract int Cost { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract CardType Type { get; }
    public abstract Rarity Rarity { get; }
    public abstract string Art { get; }

    public abstract void OnPlay(Player player);
    public abstract void OnDiscard(Player player);
    public abstract void OnDrawn(Player player);
    public abstract void OnDestroy(Player player);
}