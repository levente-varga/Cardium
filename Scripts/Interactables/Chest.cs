using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Chest : Interactable {
  public readonly List<Card> Content = GenerateLoot();

  public override void _Ready() {
    base._Ready();
    SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Chest.png"), 6, 12, false, false);
  }
  
  public override void OnInteract(Player player, World world) {
    base.OnInteract(player, world);

    if (Interacted) return;

    Interacted = true;
    Statistics.ChestsOpened++;
    world.Camera.Shake(10);
    Play("open");
    SpawnFallingLabel("Opened!");

    player.PickUpCards(Content);
  }
  
  private static List<Card> GenerateLoot() {
    List<Card> loot = new();
    Random random = new();
      
    var indexCount = random.Next(3, 8);
    for (var i = 0; i < indexCount; i++) {
      loot.Add(
        random.Next(215) switch {
          < 40 => new HealCard(),
          < 80 => new HurlCard(),
          < 120 => new SmiteCard(),
          < 140 => new ChainCard(),
          < 160 => new ShuffleCard(),
          < 180 => new TeleportCard(),
          < 190 => new HolyCard(),
          < 200 => new WoodenKeyCard(),
          < 205 => new RestCard(),
          < 210 => new EscapeCard(),
          < 215 => new GoldenKeyCard(),
          _ => new HealCard(),
        }
      );
    }

    return loot;
  }
}