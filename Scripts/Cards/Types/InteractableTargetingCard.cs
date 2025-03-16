namespace Cardium.Scripts.Cards.Types;

public partial class InteractableTargetingCard : TargetingCard
{
    public virtual bool OnPlay(Player player, Interactable target, World world) => true;
}