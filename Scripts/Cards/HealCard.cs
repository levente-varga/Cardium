using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class HealCard : PlayerTargetingCard {
    public int HealAmount = 3;
    
    public HealCard()
    {
        DisplayName = "Heal";
        Description = $"Heals for {HealAmount} missing health.";
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Heal.png");
        Type = CardType.Combat;
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override bool OnPlay(Player player)
    {
        player.Heal(HealAmount);
        
        return true;
    }
}