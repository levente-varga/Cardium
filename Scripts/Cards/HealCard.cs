using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class HealCard : PlayerTargetingCard {
  private int HealAmount => new List<int> { 1, 2, 4, 8 }[Level];

  public HealCard() {
    Name = "Heal";
    Rarity = Rarities.Common;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Heal.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Heals for {Highlight($"{HealAmount}")} missing health.";
  }

  public override bool OnPlay(Player player, World world) {
    player.Heal(HealAmount);

    return true;
  }
}