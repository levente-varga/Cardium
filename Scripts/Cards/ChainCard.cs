using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class ChainCard : EnemyTargetingCard
{
    public int Damage { get; set; } = 2;
    public int BounceRange { get; set; } = 2;
    public int Bounces { get; set; } = 2;

    private Random _random = new();
    
    public ChainCard()
    {
        DisplayName = "Chain";
        Description = $"Deals {Damage} damage to an enemy, bounces to up to {Bounces} other enemies in range {BounceRange}.";
        Cost = 2;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Chain.png");
        Type = CardType.Combat;
    }

    public override bool OnPlay(Player player, Enemy enemy, World world)
    {
        var enemies = world.GetEnemiesInRange(BounceRange, enemy.Position);
        enemies.Remove(enemy);

        void HitEnemies(int bouncesLeft, Enemy current, List<Enemy> candidates, List<Enemy> hit) {
            if (bouncesLeft <= 0) return;
            
            bouncesLeft--;
            current.ReceiveDamage(player, Damage);
            hit.Add(current);

            if (bouncesLeft <= 0) return;
            
            var otherEnemies = world.GetEnemiesInRange(BounceRange, current.Position);
            otherEnemies.RemoveAll(e => hit.Contains(e) || candidates.Contains(e));
            candidates.AddRange(otherEnemies);
            
            if (candidates.Count == 0) return;
            
            var next = candidates[_random.Next(candidates.Count)];
            candidates.Remove(next);
            HitEnemies(bouncesLeft, next, candidates, hit);
        }
        
        HitEnemies(Bounces + 1, enemy, enemies, new());

        return true;
    }
}