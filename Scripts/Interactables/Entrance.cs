using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Entrance : Interactable {
  private Player? _player;
  
  public override void _Ready() {
    base._Ready();

    SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Door.png"), 8, 12, false, false);
  }

  public override void OnNudge(Player player, Camera camera) {
    base.OnNudge(player, camera);

    if (Interacted) {
      Data.LoadDungeonData();
      GetTree().ReloadCurrentScene();
      PlayBackwards("open");
      Interacted = false;
      return;
    }

    Interacted = true;

    player.OnMoveEvent += OnPlayerMove;
    _player = player;
    
    camera.Shake(10);
    Play("open");
  }

  private void OnPlayerMove(Vector2I from, Vector2I to) {
    if (_player is null) return;
    
    _player.OnMoveEvent -= OnPlayerMove;
    _player = null;
    
    PlayBackwards("open");
    Interacted = false;
  }
}