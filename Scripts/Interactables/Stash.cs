using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Stash : Interactable {
  private Player? _player;
  
  public readonly List<Card> Content = new();

  public override void _Ready() {
    base._Ready();
    SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Chest.png"), 6, 12, false, false);
  }
  
  public override void OnNudge(Player player, Camera camera) {
    if (Interacted) return;
    
    base.OnNudge(player, camera);
    
    Blink();
    player.InventoryView.Open(true);

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