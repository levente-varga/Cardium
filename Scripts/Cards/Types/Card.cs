using System;
using Godot;

namespace Cardium.Scripts;

public class Card {
	public enum CardType {
		Combat,
		Utility,
	}
	
	public enum CardRarity {
		Common,
		Rare,
		Epic,
		Legendary,
	}
	
	public string Name { get; protected set; } = "";
	public string Description { get; protected set; } = "";
	public Texture2D Art { get; protected set; } = null!;
	public int Level { get; protected set; }
	public int MaxLevel { get; protected set; }
	public CardType Type { get; protected set; }
	public CardRarity Rarity { get; protected set; }
	public bool Unstable { get; protected set; } = false;

	protected string Highlight(string text, string color = "CCCCCC") => Highlight(text, new Color(color));
	protected string Highlight(string text, Color color) => $"[color=#{color.ToHtml()}]{text}[/color]";
	
	public virtual void OnDiscard(Player player) {}
	public virtual void OnDrawn(Player player) {}
	public virtual void OnDestroy(Player player) {}

	protected virtual void UpdateDescription() {}

	public bool Upgrade() {
		if (Level >= MaxLevel) return false;
		Level++;
		UpdateDescription();
		return true;
	}

	public Color RarityColor => Rarity switch {
		CardRarity.Common => new Color("00CB9F"),
		CardRarity.Rare => new Color("2398FF"),
		CardRarity.Epic => new Color("CB1EE9"),
		CardRarity.Legendary => new Color("FF8325"),
		_ => throw new ArgumentOutOfRangeException()
	};
}