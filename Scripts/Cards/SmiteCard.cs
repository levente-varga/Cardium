using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class SmiteCard : EnemyTargetingCard {
    private int Damage => new List<int>{5, 8, 13, 20, 30}[Level];
    
    public SmiteCard() {
        Name = "Smite";
        Description = $"Deals {Highlight($"{Damage}")} damage to a single target.";
        Rarity = CardRarity.Rare;
        Range = 3;
        MaxLevel = 4;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Smite.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player, Enemy target, World world) {
        target.ReceiveDamage(player, Damage);
        
        return true;
    }
}