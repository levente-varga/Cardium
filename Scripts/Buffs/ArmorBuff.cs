namespace Cardium.Scripts.Buffs;

public class ArmorBuff : Buff {
  public ArmorBuff(int amount, int turns) {
    Turns = turns;
  }

  public override void OnStartOfTurn() {
    base.OnStartOfTurn();
  }
}