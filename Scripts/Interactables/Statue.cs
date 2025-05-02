using System;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Statue : Interactable {
  public override void _Ready() {
    base._Ready();

    SetSprite();
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    IncreaseDifficulty();
    SetSprite();
    
    world.Camera.Shake(10);
    SpawnFloatingLabel($"{Data.Difficulty}", height: -180, lifetimeMillis: 3200);
    SpawnFloatingLabel(GetEnemyText(), height: -230, lifetimeMillis: 3200, fontSize: 24, color: Global.Gray);
    //SpawnFloatingLabel(GetLootRarityText(), height: -260, lifetimeMillis: 3200, fontSize: 24, color: Global.Gray);
  }

  private void IncreaseDifficulty() {
    var values = Enum.GetValues<Difficulty>();
    var nextIndex = ((int)Data.Difficulty + 1) % values.Length;
    Data.Difficulty = values[nextIndex];
  }

  private void SetSprite() {
    SetStillFrame(Data.Difficulty.ToString(), ResourceLoader.Load<Texture2D>($"res://Assets/Sprites/Difficulties/{Data.Difficulty.ToString()}.png"));
  }

  private string GetEnemyText() => Data.Difficulty switch {
    Difficulty.Easy => "Few, weak enemies",
    Difficulty.Moderate => "More, better enemies",
    Difficulty.Hard => "Many, strong enemies",
    _ => "Numerous, deadly enemies",
  };
  
  private string GetLootRarityText() => Data.Difficulty switch {
    Difficulty.Easy => "Common loot",
    Difficulty.Moderate => "Better loot",
    Difficulty.Hard => "Awesome loot",
    _ => "Best loot possible",
  };
}