using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Grave : Interactable {
  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Grave.png"));
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    world.Camera.Shake(10);
    SpawnFloatingLabel($"Died {(Statistics.Deaths > 1 ? $"{Statistics.Deaths} times" : "once")}");
  }
}