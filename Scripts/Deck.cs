using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;

namespace Cardium.Scripts;

public class Deck
{
    private readonly List<Card> _cards = new();
    
    public Deck(int capacity = 30)
    {
        Capacity = capacity;
    }

    private int _capacity = 1;
    public int Capacity { 
        get => _capacity;
        private set => _capacity = Math.Max(value, 1);
    }

    public int Size => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;
    public bool IsNotEmpty => _cards.Count > 0;
    public bool Contains(Card card) => _cards.Contains(card);

    /// <summary>Adds card to the bottom of the deck.</summary>
    /// <param name="card">The card to be added.</param>
    /// <returns>false if the deck is already full.</returns>
    public bool Add(Card card)
    {
        if (_cards.Count >= Capacity) return false;
        _cards.Add(card);
        return true;
    }
    
    /// <summary>Removes card from the deck.</summary>
    /// <param name="card">The card to be removed.</param>
    /// <returns>false if the card was not in the deck.</returns>
    public bool Remove(Card card)
    {
        return _cards.Remove(card);
    }
    
    public Card? Draw()
    {
        if (_cards.Count == 0)
        {
            return null;
        }
        
        var card = _cards[0];
        _cards.RemoveAt(0);
        return card;
    }
    
    public void Shuffle()
    {
        Utils.FisherYatesShuffle(_cards);        
    }
}