using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;

namespace Cardium.Scripts;

public class Deck
{
    private readonly List<Card> _cards = new();
    
    public Deck(int maxSize = 30)
    {
        MaxSize = maxSize;
    }

    private int _maxSize = 1;
    public int MaxSize { 
        get => _maxSize;
        private set => _maxSize = Math.Max(value, 1);
    }

    public int Size => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;
    public bool IsNotEmpty => _cards.Count > 0;
    public bool Contains(Card card) => _cards.Contains(card);

    public void Add(Card card)
    {
        _cards.Add(card);
    }
    
    public void Remove(Card card)
    {
        _cards.Remove(card);
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
        FisherYatesShuffle();        
    }

    private void FisherYatesShuffle()
    {
        for (var i = _cards.Count - 1; i > 0; i--)
        {
            var random = new Random();
            var j = random.Next(0, i);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }
}