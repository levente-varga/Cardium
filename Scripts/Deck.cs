using System;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public class Deck : Pile {
  public Deck(int capacity = 30) {
    Capacity = capacity;
  }

  private int _capacity = 1;

  public int Capacity {
    get => _capacity;
    private set => _capacity = Math.Max(value, 1);
  }

  public bool IsFull => Size >= Capacity;
  public bool IsNotFull => Size < Capacity;

  /// <summary>Adds card to the bottom of the deck.</summary>
  /// <param name="card">The card to be added.</param>
  /// <returns>false if the deck is already full.</returns>
  public override bool Add(Card card) {
    if (IsFull) return false;
    Cards.Add(card);
    GD.Print("Card added to deck");
    return true;
  }

  public Card? Draw() {
    if (IsEmpty) return null;
    var card = Cards.First();
    return Remove(card) ? card : null;
  }

  public void Shuffle() {
    Utils.FisherYatesShuffle(Cards);
  }
}