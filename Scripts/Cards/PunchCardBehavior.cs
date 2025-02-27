namespace Cardium.Scripts.Cards;

public class PunchCardBehavior : CardBehavior
{
    public override int Cost => 1;
    public override string Name => "Punch";
    public override string Description => "Deal 1 damage to an enemy.";
    public override CardType Type => CardType.Combat;
    public override Rarity Rarity => Rarity.Common;
    public override string Art => "res://Assets/Cards/Punch.png";
    
    public override void OnPlay(Player player)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDiscard(Player player)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDrawn(Player player)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDestroy(Player player)
    {
        throw new System.NotImplementedException();
    }
}