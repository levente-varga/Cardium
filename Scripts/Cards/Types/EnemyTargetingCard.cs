namespace Cardium.Scripts.Cards.Types;

public partial class EnemyTargetingCard : TargetingCard
{
    public virtual bool OnPlay(Player player, Enemy target, World world) => true;

}