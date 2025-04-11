using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class KeyCard : InteractableTargetingCard
{
    public override void _Ready()
    {
        DisplayName = "Key";
        Description = "Unlocks a door or chest.";
        Range = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Key.png");
        Type = CardType.Utility;
        
        base._Ready();
    }

    public override bool OnPlay(Player player, Interactable target, World world)
    {
        switch (target)
        {
            case Door door:
                door.OnInteract(player, world.Camera);
                break;
            case Chest chest:
                chest.OnInteract(player, world.Camera);
                break;
            default: return false;
        }

        return true;
    }
}