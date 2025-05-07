using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Sign : Interactable {
  public string Text = "";

  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Sign.png"));
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    world.Camera.Shake(10);
    if (Text != "") SpawnFloatingLabel(Text);
  }
}