using Cardium.Scripts.Cards.Types;

namespace Cardium.Scripts.Cards;

public partial class KeyCard : InteractableTargetingCard
{
    public override bool OnPlay(Player player, Interactable target, World world)
    {
        base.OnPlay(player, target, world);
        
        return true;
    }
}