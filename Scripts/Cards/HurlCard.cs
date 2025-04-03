using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class HurlCard : LocationTargetingCard
{
    public int Radius { get; protected set; }

    public HurlCard()
    {
        DisplayName = "Hurl";
        Description = "Deals 2 damage to all enemies in an area.";
        Cost = 3;
        Range = 3;
        Radius = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Hurl.png");
        Type = CardType.Combat;
    }
    
    public override void _Ready()
    {
        base._Ready();
    }

    public override List<Vector2I> GetHighlightedTiles(Player player, Vector2I selectedTile, World world)
    {
        return World.GetTilesInRange(selectedTile, Radius);
    }

    public override bool OnPlay(Player player, Vector2I position, World world)
    {
        List<Enemy> enemies = new ();
        foreach (var location in World.GetTilesInRange(position, Radius))
        {
            var enemy = world.GetEnemyAt(location);
            if (enemy is not null)
            {
                enemies.Add(enemy);
            }
        }
        foreach (var enemy in enemies)
        {
            enemy.ReceiveDamage(player, 2);
        }
        
        return true;
    }
}