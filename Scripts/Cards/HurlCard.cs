using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class HurlCard : LocationTargetingCard
{
    public override void _Ready()
    {
        Name = "Hurl";
        Description = "Deals 2 damage to all enemies in an area.";
        Cost = 3;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Hurl.png");
        Type = CardType.Combat;
        
        base._Ready();
    }
    
    public override void OnPlay(Player player, Vector2I position, World world)
    {
        // TODO: Implement
    }
}