using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class HolyCard : LocationTargetingCard {
  public int Radius { get; protected set; } = 2;
  public int TotalDamage { get; protected set; } = 10;

  public HolyCard()
  {
    DisplayName = "Holy";
    Description = $"Deals a total of {TotalDamage} damage to all enemies in an area, distributed equally.";
    Range = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Holy.png");
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
    var enemies = world.GetEnemiesInRange(Radius, position);
    Utils.FisherYatesShuffle(enemies);
    
    if (enemies.Count == 0) return false;
    
    var remainder = TotalDamage % enemies.Count;
    var baseDamagePerEnemy = TotalDamage / enemies.Count;
    for (var i = 0; i < enemies.Count; i++) {
      enemies[i].ReceiveDamage(player,  baseDamagePerEnemy + (i < remainder ? 1 : 0));
    }
    
    return true;
  }
}