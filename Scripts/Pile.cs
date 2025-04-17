using System.Collections.Generic;

namespace Cardium.Scripts;

public class Pile
{
    private readonly List<Cards.Types.CardView> _cards = new();

    public void Add(Cards.Types.CardView cardView)
    {
        _cards.Add(cardView);
    }

    public void Clear()
    {
        _cards.Clear();
    }
    
    public void Remove(Cards.Types.CardView cardView)
    {
        _cards.Remove(cardView);
    }
    
    public int Size => _cards.Count;
    public int IsEmpty => _cards.Count;
    public int IsNotEmpty => _cards.Count;
    
    public bool Contains(Cards.Types.CardView cardView)
    {
        return _cards.Contains(cardView);
    }
}