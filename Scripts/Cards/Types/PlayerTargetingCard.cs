namespace Cardium.Scripts.Cards.Types;

public partial class PlayerTargetingCard : Card {
  public virtual bool OnPlay(Player player) => true;
}