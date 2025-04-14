using System.Collections.Generic;

namespace Cardium.Scripts;

public class Pile
{
    private readonly List<Cards.Types.Card> _cards = new();

    public void Add(Cards.Types.Card card)
    {
        _cards.Add(card);
    }

    public void Clear()
    {
        _cards.Clear();
    }
    
    public void Remove(Cards.Types.Card card)
    {
        _cards.Remove(card);
    }
    
    public int Size => _cards.Count;
    public int IsEmpty => _cards.Count;
    public int IsNotEmpty => _cards.Count;
    
    public bool Contains(Cards.Types.Card card)
    {
        return _cards.Contains(card);
    }
}