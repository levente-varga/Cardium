using System.Collections.Generic;

namespace Cardium.Scripts;

public class Pile {
  protected readonly List<Card> _cards = new();
  public List<Card> Cards => new(_cards);
  
  public int Size => _cards.Count;
  public bool IsEmpty => _cards.Count <= 0;
  public bool IsNotEmpty => _cards.Count > 0;

  public virtual bool Add(Card card) {
    _cards.Add(card);
    return true;
  }
  public bool Remove(Card card) => _cards.Remove(card);
  public void Clear() => _cards.Clear();
  public bool Contains(Card card) => _cards.Contains(card);
  public int IndexOf(Card card) => _cards.IndexOf(card);
}