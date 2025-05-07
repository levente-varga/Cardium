using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class ChainCard : EnemyTargetingCard {
  private int Damage => new List<int> { 3, 5, 8, 15 }[Level];
  private int BounceRange => new List<int> { 3, 3, 4, 5 }[Level];
  private int Bounces => new List<int> { 1, 2, 2, 3 }[Level];
  public override int Range => new List<int> { 3, 3, 4, 5 }[Level];

  private readonly Random _random = new();

  public ChainCard() {
    Name = "Chain";
    Rarity = Rarities.Epic;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Chain.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description =
      $"Deals {Highlight($"{Damage}")} damage to an enemy, bounces to up to {Highlight($"{Bounces}")} other enemies in range {Highlight($"{BounceRange}")}.";
  }

  public override bool OnPlay(Player player, Enemy enemy, World world) {
    var enemies = world.GetEnemiesInRange(BounceRange, enemy.Position);
    enemies.Remove(enemy);

    void HitEnemies(int bouncesLeft, Enemy current, List<Enemy> candidates, List<Enemy> hit) {
      if (bouncesLeft <= 0) return;

      bouncesLeft--;
      current.ReceiveDamage(player, Damage, world);
      hit.Add(current);

      if (bouncesLeft <= 0) return;

      var otherEnemies = world.GetEnemiesInRange(BounceRange, current.Position);
      otherEnemies.RemoveAll(e => hit.Contains(e) || candidates.Contains(e));
      candidates.AddRange(otherEnemies);

      if (candidates.Count == 0) return;

      var next = candidates[_random.Next(candidates.Count)];
      candidates.Remove(next);
      HitEnemies(bouncesLeft, next, candidates, hit);
    }

    HitEnemies(Bounces + 1, enemy, enemies, new());

    return true;
  }
}