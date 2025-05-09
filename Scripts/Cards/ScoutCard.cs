using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ScoutCard : PlayerTargetingCard {
  private int Range => new List<int> { 8, 11, 15, 20 }[Level];

  public ScoutCard() {
    Name = "Scout";
    Rarity = Rarities.Rare;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Scout.png");
    Type = Types.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Discovers area in a {Highlight($"{Range}")} radius.";
  }

  public override bool OnPlay(Player player, World world) {
    world.DimFogOfWarBetweenRanges(player.Position, player.Vision, Range, false);

    return true;
  }
}