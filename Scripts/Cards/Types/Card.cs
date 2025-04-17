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
}