using System.Collections.Generic;

namespace Cardium.Scripts;

public class Pile
{
    private readonly List<Card> _cards = new();

    public void Add(Card card)
    {
        _cards.Add(card);
    }

    public void Clear()
    {
        _cards.Clear();
    }
    
    public void Remove(Card card)
    {
        _cards.Remove(card);
    }
    
    public int Size => _cards.Count;
    
    public bool Contains(Card card)
    {
        return _cards.Contains(card);
    }
}