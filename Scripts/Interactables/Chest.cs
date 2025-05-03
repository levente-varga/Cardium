using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Chest : Interactable {
  public readonly List<Card> Content = GenerateLoot;

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
  
  private static List<Card> GenerateLoot => Utils.GenerateLoot(
    Data.Difficulty switch {
      Difficulty.Easy => Global.Random.Next(3, 8),
      Difficulty.Moderate => Global.Random.Next(4, 10),
      Difficulty.Hard => Global.Random.Next(5, 12),
      _ => Global.Random.Next(6, 15),
    },
    new Dictionary<Type, int> {
      { typeof(HealCard), 40 },
      { typeof(ShieldCard), 20 },
      { typeof(SmiteCard), 40 },
      { typeof(ChainCard), 40 },
      { typeof(ShuffleCard), 5 },
      { typeof(TeleportCard), 20 },
      { typeof(HolyCard), 10 },
      { typeof(RestCard), 5 },
      { typeof(EscapeCard), 5 },
      { typeof(GuideCard), 5 },
    }
  );
}