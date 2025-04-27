using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Voidling : Enemy {
  protected override int MaxLevel => 2;
  
  
  private bool Stealth { set; get; } = true;

  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Voidling.png"), 8, 12);

    Name = "Voidling";
    Description = "Untargetable until it attacks.";
    MaxHealth = new List<int> { 5, 7, 9, 11, 13 }[Level];
    Health = MaxHealth;
    BaseVision = 5;
    BaseCombatVision = 7;
    BaseArmor = 0;
    BaseDamage = new List<int> { 3, 5, 7, 9, 11 }[Level];
    BaseRange = 1;

    SetModulate();
  }

  protected override void TakeTurn(Player player, World world) {
    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    Invincible = Stealth;

    SetModulate();

    base.TakeTurn(player, world);

    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    Invincible = Stealth;

    SetModulate();
  }

  private void SetModulate() {
    Modulate = new Color(1, 1, 1, Stealth ? 0.25f : 1);
  }
  
  protected override List<Card> GenerateLoot() {
    List<Card> loot = new();
    Random random = new();
      
    var indexCount = random.Next(1, 3);
    for (var i = 0; i < indexCount; i++) {
      loot.Add(
        random.Next(120) switch {
          < 40 => new GoldenKeyCard(),
          < 60 => new HolyCard(),
          < 80 => new SmiteCard(),
          < 100 => new HurlCard(),
          < 110 => new RestCard(),
          < 120 => new EscapeCard(),
          _ => new HealCard(),
        }
      );
    }

    return loot;
  }
}