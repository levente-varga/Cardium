using Godot;

namespace Cardium.Scripts;

public partial class CardLoot : TileAlignedGameObject
{
    public Cards.Types.Card Card;

    public override void _Ready()
    {
        base._Ready();
        
        SetAnimation("idle", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Card.png"), 8, 12);
    }
}