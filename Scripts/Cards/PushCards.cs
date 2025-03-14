using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class PushCard : DirectionalCard
{
    public PushCard()
    {
        DisplayName = "Push";
        Description = "Pushes an enemy away 2 tiles. Deals 1 damage. Enemies in the path also take 1 damage.";
        Cost = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Push.png");
        Type = CardType.Combat;
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void OnPlay(Player player, Direction direction, World world)
    {
        // TODO: Implement
    }
}