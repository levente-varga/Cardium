using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class HealCard : PlayerTargetingCard
{
    public override void _Ready()
    {
        Name = "Heal";
        Description = "Heals for 3 missing health.";
        Cost = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Heal.png");
        Type = CardType.Combat;
        
        base._Ready();
    }
    
    public override void OnPlay(Player player)
    {
        player.Heal(3);
    }
}