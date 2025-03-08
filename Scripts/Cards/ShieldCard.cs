using Godot;

namespace Cardium.Scripts.Cards;

public partial class ShieldCard : Card
{
    public override void _Ready()
    {
        Name = "Shield";
        Description = "Raises Armor by 2 for 3 turns.";
        Cost = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shield.png");
        Type = CardType.Combat;
        
        base._Ready();
    }
    
    public override void OnPlay(Player player)
    {
        // TODO: Implement
    }
}