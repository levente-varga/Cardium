using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts;

public partial class DeckView : Node2D {
  [Export] private PackedScene _cardBackScene = ResourceLoader.Load<PackedScene>("res://Scenes/card_back.tscn");
  [Export] private Label SizeLabel = null!;

  private readonly List<Node2D> _cardBackViews = new();
  public Deck Deck { get; } = new();

  public override void _Process(double delta) {
    SizeLabel.Text = Deck.IsEmpty ? "" : $"{Deck.Size}";
  }

  public bool Add(Card card) {
    if (!Deck.Add(card)) return false;
    GD.Print("Card added to deck view");
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
    Add(new HealCard { Protected = true });
    Add(new HurlCard { Protected = true });
    Add(new SmiteCard { Protected = true });
    Add(new WoodenKeyCard { Protected = true });
    Add(new RestCard());
    Add(new ShuffleCard { Protected = true });
    Deck.Shuffle();
  }
}