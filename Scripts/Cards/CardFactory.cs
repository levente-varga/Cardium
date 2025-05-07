using System;
using Cardium.Scripts.Cards.Types;

namespace Cardium.Scripts.Cards;

public static class CardFactory {
  public static Card Create(string name) {
    return name.ToLower() switch {
      "chain" => new ChainCard(),
      "escape" => new EscapeCard(),
      "golden key" => new GoldenKeyCard(),
      "guide" => new GuideCard(),
      "heal" => new HealCard(),
      "holy" => new HolyCard(),
      "hurl" => new HurlCard(),
      "rest" => new RestCard(),
      "scout" => new ScoutCard(),
      "shield" => new ShieldCard(),
      "shuffle" => new ShuffleCard(),
      "smite" => new SmiteCard(),
      "teleport" => new TeleportCard(),
      "wooden key" => new WoodenKeyCard(),
      _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };
  }
}