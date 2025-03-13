using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class LocationTargetingCard : TargetingCard
{
    public virtual void OnPlay(Player player, Vector2I position, World world) {}
}