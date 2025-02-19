using System;
using System.Collections.Generic;

namespace Cardium.Scripts;

public class Deck
{
    private List<Card> cards = new();
    
    public void Shuffle ()
    {
        // Fisher-Yates shuffle algorithm
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var random = new Random();
            var j = random.Next(0, i);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public void Add(Card card)
    {
        cards.Add(card);
    }
    
    public void Remove(Card card)
    {
        cards.Remove(card);
    }
    
    public Card Draw()
    {
        if (cards.Count == 0)
        {
            return null;
        }
        
        var card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
    
    public int Size => cards.Count;
    
    public bool Contains(Card card)
    {
        return cards.Contains(card);
    }
}