using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public class Pile {
  protected readonly List<Card> Cards = new();
  public List<Card> GetCards() => new(Cards);

  public List<Card> GetCardsOrdered() {
    var cards = new List<Card>(Cards);
    cards.Sort((card, other) => {
      var nameOrder = string.Compare(card.Name, other.Name, StringComparison.Ordinal);
      var result = nameOrder == 0 ? card.Level.CompareTo(other.Level) : nameOrder;
      GD.Print($"Comparing {card.Name} to {other.Name} and {card.Level} to {other.Level} => {result}");
      return result;
    });
    GD.Print("Result:");
    foreach (var card in cards) GD.Print($"{card.Name} (lvl. {card.Level})");
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