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
    SpawnFloatingLabel($"{Data.Difficulty}", height: -170, lifetimeMillis: 3200, color: GetDifficultyColor());
    SpawnFloatingLabel(GetEnemyText(), height: -220, lifetimeMillis: 3200, fontSize: 24, color: Global.Gray);
    SpawnFloatingLabel(GetBonfireLadderText(), height: -250, lifetimeMillis: 3200, fontSize: 24, color: Global.Gray);
    SpawnFloatingLabel(GetLootRarityText(), height: -280, lifetimeMillis: 3200, fontSize: 24, color: Global.Gray);
  }

  private void IncreaseDifficulty() {
    var values = Enum.GetValues<Difficulty>();
    var nextIndex = ((int)Data.Difficulty + 1) % values.Length;
    Data.Difficulty = values[nextIndex];
  }

  private void SetSprite() {
    SetStillFrame(Data.Difficulty.ToString(), ResourceLoader.Load<Texture2D>($"res://Assets/Sprites/Difficulties/{Data.Difficulty.ToString()}.png"));
  }

  private Color GetDifficultyColor() => Data.Difficulty switch {
    Difficulty.Easy => Global.Green,
    Difficulty.Moderate => Global.Yellow,
    Difficulty.Hard => Global.Red,
    Difficulty.Brutal => Global.Orange,
  };
  
  private string GetEnemyText() => Data.Difficulty switch {
    Difficulty.Easy => "Few, weak enemies",
    Difficulty.Moderate => "More, better enemies",
    Difficulty.Hard => "Many, strong enemies",
    _ => "Numerous, deadly enemies",
  };
  
  private string GetBonfireLadderText() => Data.Difficulty switch {
    Difficulty.Easy => "Frequent bonfires & ladders",
    Difficulty.Moderate => "Regular bonfires & ladders",
    Difficulty.Hard => "Occasional bonfires & ladders",
    _ => "Sparse bonfires & ladders",
  };
  
  private string GetLootRarityText() => Data.Difficulty switch {
    Difficulty.Easy => "Very little loot",
    Difficulty.Moderate => "Average loot",
    Difficulty.Hard => "More loot",
    _ => "Tons of loot",
  };
}