using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class DeckView : Node2D {
  [Export] private PackedScene _cardBackScene = ResourceLoader.Load<PackedScene>("res://Scenes/card_back.tscn");
  [Export] private Label SizeLabel = null!;

  private readonly List<Node2D> _cardBackViews = new();
  public Pile Deck { get; } = new () { Capacity = 20 };

  public override void _Process(double delta) {
    SizeLabel.Text = Deck.IsEmpty ? "" : $"{Deck.Size}";
  }

  public bool Add(Card card) {
    if (!Deck.Add(card)) return false;
    var view = _cardBackScene.Instantiate<Node2D>();
    _cardBackViews.Add(view);
    PositionCardBackViews();
    AddChild(view);
    return true;
  }

  public bool Remove(Card card) {
    if (!Deck.Remove(card)) return false;
    RemoveCardView();
    return true;
  }

  public void Clear() => Deck.Clear();

  private void PositionCardBackViews() {
    for (var i = 0; i < _cardBackViews.Count; i++) {
      var index = _cardBackViews.Count - i - 1;
      _cardBackViews[index].RotationDegrees = i * 1.5f;
      _cardBackViews[index].Position = Vector2.One * i * 6;
    }
  }

  public Card? DrawCard() {
    var card = Deck.Draw();
    if (card != null) {
      RemoveCardView();
    }

    return card;
  }

  public void RemoveCardView() {
    _cardBackViews[0].QueueFree();
    _cardBackViews.RemoveAt(0);
    PositionCardBackViews();
  }

  public void FillWithInitial() {
    Deck.Clear();
    Add(new HealCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new HealCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new HurlCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new HurlCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new SmiteCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new SmiteCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new WoodenKeyCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new RestCard { Protected = true, Origin = Card.Origins.Deck });
    Add(new ShuffleCard { Protected = true, Origin = Card.Origins.Deck });
    Deck.Shuffle();
  }
}