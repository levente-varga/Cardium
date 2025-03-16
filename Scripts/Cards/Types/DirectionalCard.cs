namespace Cardium.Scripts.Cards.Types;

public partial class DirectionalCard : Card
{
    public virtual bool OnPlay(Player player, Direction direction, World world) => true;
}