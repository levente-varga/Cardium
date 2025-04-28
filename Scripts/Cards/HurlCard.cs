using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class HurlCard : LocationTargetingCard {
  private int Damage => new List<int> { 3, 5, 8, 11, 17 }[Level];
  private int Radius => new List<int> { 1, 1, 1, 2, 2 }[Level];

  public HurlCard() {
    Name = "Hurl";
    Rarity = CardRarity.Common;
    Range = 3;
    MaxLevel = 4;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Hurl.png");
    Type = CardType.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Deals {Highlight($"{Damage}")} damage to all enemies in a {Highlight($"{Radius}")} radius.";
  }

  public override List<Vector2I> GetHighlightedTiles(Player player, Vector2I selectedTile, World world) {
    return World.GetTilesInRange(selectedTile, Radius);
  }

  public override bool OnPlay(Player player, Vector2I position, World world) {
    List<Enemy> enemies = new();
    foreach (var location in World.GetTilesInRange(position, Radius)) {
      var enemy = world.GetEnemyAt(location);
      if (enemy is not null) {
        enemies.Add(enemy);
      }
    }

    foreach (var enemy in enemies) {
      enemy.ReceiveDamage(player, Damage, world);
    }

    return true;
  }
}