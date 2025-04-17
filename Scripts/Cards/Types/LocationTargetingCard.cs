using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class LocationTargetingCard : TargetingCard {
    public virtual bool OnPlay(Player player, Vector2I position, World world) => true;
}