using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Workbench : Interactable {
  public override void _Ready() {
    base._Ready();
    SetStillFrame(ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Workbench.png"));
  }

  public override void OnNudge(Player player, Camera camera) {
    base.OnNudge(player, camera);
    
    camera.Shake(10);
  }
}