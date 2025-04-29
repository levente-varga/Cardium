using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class HolyCard : LocationTargetingCard {
  private int Radius => new List<int> { 2, 2, 3, 4 }[Level];
  private int TotalDamage => new List<int> { 10, 16, 27, 50 }[Level];

  public HolyCard() {
    Name = "Holy";
    Rarity = CardRarity.Epic;
    Range = 3;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Holy.png");
    Type = CardType.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description =
      $"Deals a total of {Highlight($"{TotalDamage}")} damage to all enemies in a radius of {Highlight($"{Radius}")}, distributed equally.";
  }

  public override List<Vector2I> GetHighlightedTiles(Player player, Vector2I selectedTile, World world) {
    return World.GetTilesInRange(selectedTile, Radius);
  }

  public override bool OnPlay(Player player, Vector2I position, World world) {
    var enemies = world.GetEnemiesInRange(Radius, position);
    Utils.FisherYatesShuffle(enemies);

    if (enemies.Count == 0) return false;

    var remainder = TotalDamage % enemies.Count;
    var baseDamagePerEnemy = TotalDamage / enemies.Count;
    for (var i = 0; i < enemies.Count; i++) {
      enemies[i].ReceiveDamage(player, baseDamagePerEnemy + (i < remainder ? 1 : 0), world);
    }

    return true;
  }
}