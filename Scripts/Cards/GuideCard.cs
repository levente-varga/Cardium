using System.Linq;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Enemies;
using Godot;

namespace Cardium.Scripts.Cards;

public class GuideCard : PlayerTargetingCard {
  public GuideCard() {
    Name = "Guide";
    Rarity = Rarities.Legendary;
    Unstable = true;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Guide.png");
    Type = Types.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Marks the ground showing the rough direction of the Exterminator. {Highlight("Unstable")}";
  }

  public override bool OnPlay(Player player, World world) {
    var exterminator = world.Dungeon.Enemies.FirstOrDefault(enemy => enemy is Exterminator);

    if (exterminator == null) return true;
    
    var distance = exterminator.Position - player.Position;
    var direction = Utils.VectorToDirection(distance);
    var atlasIndex = direction switch {
      Direction.Up => 0,
      Direction.Right => 1,
      Direction.Down => 2,
      _ => 3
    };
    
    world.Dungeon.SetDecor(player.Position, 1, new Vector2I(atlasIndex, 0));

    return true;
  }
}