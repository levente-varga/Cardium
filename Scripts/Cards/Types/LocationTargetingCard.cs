using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class LocationTargetingCard : Card
{
    public virtual void OnPlay(Player player, Vector2I position, World world) {}
}