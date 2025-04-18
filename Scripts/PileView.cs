using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class PileView : Node2D {
	[Export] private PackedScene _cardBackScene = ResourceLoader.Load<PackedScene>("res://Scenes/card_back.tscn");
	
	public bool Enabled = true;
	
	private readonly List<Node2D> _cardBackViews = new();
	public Pile Pile { get; } = new ();
	
	public const int CardSpreadDeg = 15;
	private Random _random = new();
	
	public override void _Ready() {
		
	}

	public void Add(Card card) {
		Pile.Add(card);
		var view = _cardBackScene.Instantiate<Node2D>();
		view.RotationDegrees = _random.Next(-CardSpreadDeg, CardSpreadDeg);
		_cardBackViews.Add(view);
		AddChild(view);
	}

	public bool Remove(Card card) {
		var index = Pile.IndexOf(card);
		if (!Pile.Remove(card)) return false;
		_cardBackViews[index].QueueFree();
		_cardBackViews.RemoveAt(index);
		return true;
	}

	public void RemoveAll() {
		for (var i = 0; i < Pile.Size; i++) {
			_cardBackViews[i].QueueFree();
		}
		Pile.Clear();
		_cardBackViews.Clear();
	}
}