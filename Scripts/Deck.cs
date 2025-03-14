using System;
using System.Collections.Generic;

namespace Cardium.Scripts;

public class Deck
{
    public Deck(int maxSize = 30)
    {
        MaxSize = maxSize;
    }

    private int _maxSize = 1;
    public int MaxSize { 
        get => _maxSize;
        set => _maxSize = Math.Max(value, 1);
    }
    private readonly List<Cards.Types.Card> _cards = new();
    
    public void Shuffle ()
    {
        // Fisher-Yates shuffle algorithm
        for (var i = _cards.Count - 1; i > 0; i--)
        {
            var random = new Random();
            var j = random.Next(0, i);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    public void Add(Cards.Types.Card card)
    {
        _cards.Add(card);
    }
    
    public void Remove(Cards.Types.Card card)
    {
        _cards.Remove(card);
    }
    
    public Cards.Types.Card Draw()
    {
        if (_cards.Count == 0)
        {
            return null;
        }
        
        var card = _cards[0];
        _cards.RemoveAt(0);
        return card;
    }
    
    public int Size => _cards.Count;
    
    public bool Contains(Cards.Types.Card card)
    {
        return _cards.Contains(card);
    }
}