using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class CardLoot : TileAlignedGameObject {
  public Card Card { get; init; } = null!;

  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Card.png"), 8, 12);
  }
}