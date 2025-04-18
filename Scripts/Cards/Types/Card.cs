using Godot;

namespace Cardium.Scripts;

public class Card {
	public enum CardType {
		Combat,
		Utility,
	}
	
	public string Name { get; protected set; } = "";
	public string Description { get; protected set; } = "";
	public Texture2D Art { get; protected set; } = null!;
	public int Level { get; protected set; }
	public CardType Type { get; protected set; }
	public bool Unstable { get; protected set; } = false;

	protected string Highlight(string text, string color = "CCCCCC") => Highlight(text, new Color(color));
	protected string Highlight(string text, Color color) => $"[color=#{color.ToHtml()}]{text}[/color]";
	
	public virtual void OnDiscard(Player player) {}
	public virtual void OnDrawn(Player player) {}
	public virtual void OnDestroy(Player player) {}
}