using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Hand : Node2D
{
	[Export] public Player Player;
	[Export] public float HandRadius = 1000f;
	[Export] public float HandHeight = 64;
	[Export] public float MaxHandEnclosedAngle = 30f;
	[Export] public float DefaultCardAngle = 7f;
	[Export] public float TiltAngle = 0f;
	[Export] public Vector2 Origin = Vector2.Zero;
	
	public bool RightHanded = true;

	private bool _enabled;
	public bool Enabled
	{
		get => _enabled;
		set
		{
			_enabled = value;
			EnableCards(value);
		}
	}

	private readonly List<Card> _cards = new();
	private List<float> _cardAngles = new();
	private const int MaxHandSize = 10;
	private Rect2 _playArea;

	public override void _Ready()
	{
		Origin = new Vector2(
			0,
			GetViewport().GetVisibleRect().Size.Y - HandHeight + HandRadius / 2
		);
		
		PositionHand();
		
		_playArea = new Rect2(
			0,
			0,
			GetViewport().GetVisibleRect().Size.X,
			GetViewport().GetVisibleRect().Size.Y / (3f / 2f)
		);
	}
	
	public override void _Process(double delta)
	{
		
	}


	private void PositionHand()
	{	
		var oldCardAngles = new List<float>(_cardAngles);
		_cardAngles = GetCardAngles();

		for (var i = 0; i < _cards.Count; i++)
		{
			var tween = CreateTween();
			var index = i;
			tween.TweenMethod(Callable.From<float>(value => SetCardPosition(index, value)), oldCardAngles[index], _cardAngles[index], 0.4f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			tween.TweenCallback(Callable.From(() => { tween.Dispose(); }));
		}
	}


	private void SetCardPosition(int index, float angle) {
		if (index >= _cards.Count || index < 0) return;
		Vector2 cardPosition = GetPointOnCircle(Origin, HandRadius, angle);

		_cards[index].Position = cardPosition;
		_cards[index].Rotation = DegreeToRadian(angle + 90);
		//_cards[index].ZIndex = index;
		_cardAngles[index] = angle;
	}


	private List<float> GetCardAngles() => GetCardAngles(_cards.Count);
	private List<float> GetCardAngles(int cardCount)
	{
		var angles = new List<float>();

		var handEnclosedAngle = MathF.Min(MaxHandEnclosedAngle, (cardCount - 1) * DefaultCardAngle);
		var handStartAngle = 270 - handEnclosedAngle / 2;
		float cardAngle = 0;
		if (cardCount > 1) cardAngle = handEnclosedAngle / (cardCount - 1);

		for (var i = 0; i < cardCount; i++)
		{
			var angle = handStartAngle + cardAngle * i;
			angles.Add(angle);
		}

		return angles;
	}


	private float DegreeToRadian(float angle)
	{
		return (float)(MathF.PI * angle / 180.0);
	}


	private float RadianToDegree(float angle)
	{
		return (float)(angle * (180.0 / MathF.PI));
	}


	private Vector2 GetPointOnCircle(float radius, float angle)
	{
		return new Vector2(
			radius * MathF.Cos(DegreeToRadian(angle)),
			radius * MathF.Sin(DegreeToRadian(angle))
		);
	}


	private Vector2 GetPointOnCircle(Vector2 origin, float radius, float angle)
	{
		return origin + GetPointOnCircle(radius, angle);
	}
	
	
	public void AddCard(Card card) => AddCard(card, _cards.Count);
	public void AddCard(Card card, int index)
	{
		if (_cards.Count >= MaxHandSize) return;
		_cardAngles.Add(315);
		_cards.Insert(index, card);
		AddChild(card);
		PositionHand();
		
		card.OnDragEndEvent += OnCardDragEnd;
		card.OnDragEvent += OnCardDrag;
	}

	private void RemoveLastCard() => RemoveCard(_cards.Last());
	private void RemoveCard(int index) => RemoveCard(_cards[index]);
	private void RemoveCard(Card card)
	{
		if (!_cards.Contains(card)) return;
		_cardAngles.RemoveAt(_cards.IndexOf(card));
		card.OnDragEndEvent -= OnCardDragEnd;
		card.OnDragEvent -= OnCardDrag;
		_cards.Remove(card);
		card.QueueFree();
		PositionHand();
	}


	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventKey { Pressed: true, KeyLabel: Key.Key1 }:
				AddCard(new HealCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key2 }:
				AddCard(new SmiteCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key3 }:
				AddCard(new HurlCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key4 }:
				AddCard(new PushCard());
				break;
			case InputEventKey { Pressed: true, KeyLabel: Key.Key5 }: 
				AddCard(new ChainCard());
				break;
		}
	}

	private void OnCardDrag(Card card, Vector2 mousePosition)
	{
		switch (card.InPlayArea)
		{
			case false when _playArea.HasPoint(mousePosition):
				card.OnEnterPlayArea();
				Utils.SpawnFallingLabel(GetTree(), card.GlobalPosition, "Entered play area");
				break;
			case true when !_playArea.HasPoint(mousePosition):
				card.OnExitPlayArea();
				Utils.SpawnFallingLabel(GetTree(), card.GlobalPosition, "Exited play area");
				break;
		}
	}
	
	private void OnCardDragEnd(Card card, Vector2 mousePosition)
	{
		if (card.InPlayArea) card.OnExitPlayArea();
		if (!_playArea.HasPoint(mousePosition)) return;
		if (Player.Energy < card.Cost)
		{
			Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Not enough energy!", color: Global.Purple);
			return;
		}
		
		card.OnEnterPlayArea();
		PlayCard(card);
	}

	private async Task PlayCard(Card card)
	{
		Enabled = false;
		var success = await Player.World.PlayCard(card);
		Enabled = true;

		if (success)
		{
			Player.SpendEnergy(card.Cost);
			RemoveCard(card);
			PositionHand();
		}
		else card.OnExitPlayArea();
		
		Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Play: " + (success ? "success" : "cancelled"));
	}
	
	private void EnableCards(bool enable)
	{
		foreach (var card in _cards)
		{
			card.Enabled = enable;
		}
		
		Utils.SpawnFallingLabel(GetTree(), Player.GlobalPosition, "Hand " + (enable ? "enabled" : "disabled"));
	}
}