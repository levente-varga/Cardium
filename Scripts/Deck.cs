using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public class Deck
{
    private readonly List<Card> _cards = new();
    public List<Card> Cards => new(_cards);
    
    public Deck(int capacity = 30) {
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
    public bool Add(Card card) {
        if (_cards.Count >= Capacity) {
            GD.Print($"Couldn't add card to deck, its full");
            return false;
        }
        GD.Print($"Added a card to the deck");
        _cards.Add(card);
        return true;
    }
    
    /// <summary>Removes card from the deck.</summary>
    /// <param name="card">The card to be removed.</param>
    /// <returns>false if the card was not in the deck.</returns>
    public bool Remove(Card card) {
        return _cards.Remove(card);
    }
    
    public Card? Draw() {
        if (_cards.Count == 0) return null;
        
        GD.Print($"Drawn a card from the deck");
        
        var card = _cards.First();
        Remove(card);
        return card;
    }
    
    public void Shuffle() {
        Utils.FisherYatesShuffle(_cards);        
    }
}