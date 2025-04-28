using System.Collections.Generic;
using System.Linq;

namespace Cardium.Scripts;

public class Pile {
  protected readonly List<Card> Cards = new();
  public List<Card> GetCards() => new(Cards);

  public int Size => Cards.Count;
  public bool IsEmpty => Cards.Count <= 0;
  public bool IsNotEmpty => Cards.Count > 0;
  public int UnprotectedCount => Cards.Where(card => !card.Protected).ToArray().Length;

  public virtual void AddAll(List<Card> cards) {
    foreach (var card in cards) Add(card);
  }

  public virtual bool Add(Card card) {
    Cards.Add(card);
    return true;
  }

  public bool Remove(Card card) => RemoveAt(Cards.IndexOf(card));

  public bool RemoveAt(int index) {
    if (index < 0 || index >= Cards.Count) return false;
    Cards.RemoveAt(index);
    return true;
  }

  public void RemoveUnprotected() {
    Cards.RemoveAll(card => !card.Protected);
  }

  public void Clear() => Cards.Clear();
  public bool Contains(Card card) => Cards.Contains(card);
  public int IndexOf(Card card) => Cards.IndexOf(card);
}