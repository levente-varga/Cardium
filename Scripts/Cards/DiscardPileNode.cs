using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class DiscardPileNode : Node2D {
  public Pile Pile = new();

  private List<Sprite2D> _cards;
  
  
}