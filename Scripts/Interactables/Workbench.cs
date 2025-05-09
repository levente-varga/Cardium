using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Workbench : Interactable {
  private Player? _player;

  public override void _Ready() {
    base._Ready();
    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Workbench.png"));
  }

  public override void OnNudge(Player player, World world) {
    if (Interacted) return;

    base.OnNudge(player, world);

    Blink();
    world.WorkbenchMenu.Open();

    Interacted = true;

    player.OnMoveEvent += OnPlayerMove;
    _player = player;

    world.Camera.Shake(10);
    //Play("open");
  }

  private void OnPlayerMove(Vector2I from, Vector2I to) {
    if (_player is null) return;

    _player.OnMoveEvent -= OnPlayerMove;
    _player = null;

    //PlayBackwards("open");
    Interacted = false;
  }
}