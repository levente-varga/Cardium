using System;
using System.Linq;

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

  /// <summary>Adds card to the bottom of the deck.</summary>
  /// <param name="card">The card to be added.</param>
  /// <returns>false if the deck is already full.</returns>
  public override bool Add(Card card) {
    if (_cards.Count >= Capacity) return false;
    _cards.Add(card);
    return true;
  }

  public Card? Draw() {
    if (IsEmpty) return null;
    var card = _cards.First();
    return Remove(card) ? card : null;
  }

  public void Shuffle() {
    Utils.FisherYatesShuffle(_cards);        
  }
}