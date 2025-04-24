using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Exit : Interactable {
  private int _lastPlayerTurnsLived = -2;
  
  public override void _Ready() {
    base._Ready();

    SetStillFrame(ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Ladder.png"));
  }

  public override void OnNudge(Player player, Camera camera) {
    base.OnNudge(player, camera);
    
    if (player.TurnsLived - _lastPlayerTurnsLived == 1) {
      GetTree().Quit();
      return;
    }
    
    Blink();
    
    _lastPlayerTurnsLived = player.TurnsLived;
    
    camera.Shake(10);
    SpawnFloatingLabel("Quit game?", height: Global.GlobalTileSize.Y * 3);
  }
}