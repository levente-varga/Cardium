using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class PushCard : EnemyTargetingCard
{
    public PushCard()
    {
        Name = "Push";
        Description = "Pushes an enemy away 2 tiles. Deals 1 damage. Enemies in the path also take 1 damage.";
        Cost = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Push.png");
        Type = CardType.Combat;
        Range = 1;
    }
    
    public override void _Ready()
    {
        
        
        base._Ready();
    }
    
    public override void OnPlay(Player player, Enemy target)
    {
        // TODO: Implement
    }
}