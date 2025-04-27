using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class TargetingCard : Card {
  public virtual int Range { get; protected set; }

  public virtual List<Vector2I> GetHighlightedTiles(Player player, Vector2I selectedTile, World world) {
    return new List<Vector2I> { selectedTile };
  }
}