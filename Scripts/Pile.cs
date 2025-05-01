using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public class Pile {
  public readonly List<Card> Cards = new();
  
  private int? _capacity;
  public int? Capacity {
    get => _capacity;
    set => _capacity = value == null ? null : Mathf.Max(value.Value, 1);
  }

  public bool IsFull => Capacity != null && Size >= Capacity;
  public bool IsNotFull => Capacity == null || Size < Capacity;

  public List<Card> GetCardsSorted() {
    var cards = new List<Card>(Cards);
    Utils.SortCards(cards);
    return cards;
  }

  public int Size => Cards.Count;
  public bool IsEmpty => Cards.Count <= 0;
  public bool IsNotEmpty => Cards.Count > 0;
  public int UnprotectedCount => Cards.Where(card => !card.Protected).ToArray().Length;

  public virtual void AddAll(List<Card> cards) {
    foreach (var card in cards) Add(card);
  }

  public virtual bool Add(Card card) {
    if (IsFull) return false;
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
  
  public Card? Draw() {
    if (IsEmpty) return null;
    var card = Cards.First();
    return Remove(card) ? card : null;
  }

  public void Shuffle() {
    Utils.FisherYatesShuffle(Cards);
  }
}