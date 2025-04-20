using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Stash : Interactable {
  public readonly List<Card> Content = new();

  public override void _Ready() {
    base._Ready();
    SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Chest.png"), 6, 12, false, false);
  }

  public override void _Process(double delta) {
    base._Process(delta);
  }

  public override void OnNudge(Player player, Camera camera) {
    base.OnNudge(player, camera);
    
    Blink();
    
    camera.Shake(10);
    Stop();
    Play("open");
  }
}