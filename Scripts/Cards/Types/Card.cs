using System;
using Godot;

namespace Cardium.Scripts.Cards.Types;

public class Card {
  public enum Types {
    Combat,
    Utility,
  }

  public enum Rarities {
    Common,
    Rare,
    Epic,
    Legendary,
  }
  
  public enum Origins {
    None,
    Deck,
    Stash,
    Inventory,
  }
  
  private Origins _origin = Origins.None;
  public Origins Origin {
    get => _origin;
    set {
      if (value == _origin) return;
      _origin = value;
      OnOriginChangedEvent?.Invoke(value);
    }
  }

  public bool OnMaxLevel => Level >= MaxLevel;
  
  public delegate void OnOriginChangedDelegate(Origins newOrigin);
  public event OnOriginChangedDelegate? OnOriginChangedEvent;

  public string Name { get; protected set; } = "";
  public string Description { get; protected set; } = "";
  public Texture2D Art { get; protected set; } = null!;
  public int Level { get; protected set; }
  public int MaxLevel { get; protected set; }
  public Types Type { get; protected set; }
  public Rarities Rarity { get; protected set; }
  public bool Unstable { get; protected set; } = false;
  public bool Protected { get; set; } = false;

  protected static string Highlight(string text, string color = "CCCCCC") => Highlight(text, new Color(color));
  protected static string Highlight(string text, Color color) => $"[color=#{color.ToHtml()}]{text}[/color]";

  public virtual void OnDiscard(Player player) {
  }

  public virtual void OnDrawn(Player player) {
  }

  public virtual void OnDestroy(Player player) {
  }

  protected virtual void UpdateDescription() {
  }

  public bool Upgrade() {
    if (Level >= MaxLevel) return false;
    Level++;
    UpdateDescription();
    return true;
  }

  public Color RarityColor => Rarity switch {
    Rarities.Common => new Color("CCCCCC"),
    Rarities.Rare => new Color("2398FF"),
    Rarities.Epic => new Color("CB1EE9"),
    Rarities.Legendary => new Color("FF8325"),
    _ => throw new ArgumentOutOfRangeException()
  };
  
  public Godot.Collections.Dictionary Save() {
    return new Godot.Collections.Dictionary {
      {"name", Name.ToLower()},
      {"level", Level},
      {"protected", Protected}
    };
  }
}