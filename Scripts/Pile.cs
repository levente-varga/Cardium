using System.Collections.Generic;

namespace Cardium.Scripts;

public class Pile {
  private readonly List<Card> _cards = new();
  
  public int Size => _cards.Count;
  public bool IsEmpty => _cards.Count <= 0;
  public bool IsNotEmpty => _cards.Count > 0;

  public void Add(Card card) => _cards.Add(card);
  public bool Remove(Card card) => _cards.Remove(card);
  public void Clear() => _cards.Clear();
  public bool Contains(Card card) => _cards.Contains(card);
  public int IndexOf(Card card) => _cards.IndexOf(card);
}