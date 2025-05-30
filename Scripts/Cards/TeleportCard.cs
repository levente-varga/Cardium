using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class TeleportCard : LocationTargetingCard {
  public override int Range => new List<int> { 2, 3, 4, 5 }[Level];

  public TeleportCard() {
    Name = "Teleport";
    Rarity = Rarities.Epic;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Teleport.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Teleports player to an empty location within {Highlight($"{Range}")} radius.";
  }

  public override bool OnPlay(Player player, Vector2I position, World world) {
    if (!world.IsEmpty(position)) {
      player.SpawnFloatingLabel("Occupied!", Global.Red, height: 184);
      return false;
    }
    
    player.SetPosition(position);

    return true;
  }
}