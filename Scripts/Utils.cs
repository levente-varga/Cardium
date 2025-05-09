using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Labels;
using Godot;

namespace Cardium.Scripts;

public static class Utils {
  public static int ManhattanDistanceBetween(Vector2I a, Vector2I b) {
    return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
  }

  public static void SpawnFallingLabel(Node parent, Vector2 position, string text, Color? color = null,
    int? fontSize = null, int? lifetimeMillis = null) {
    parent.AddChild(new FallingLabel {
      Text = text,
      Position = position,
      Color = color,
      FontSize = fontSize,
      LifetimeMillis = lifetimeMillis,
    });
  }

  public static void SpawnFloatingLabel(Node parent, Vector2 position, string text, Color? color = null,
    int height = 120, int? fontSize = null, int? lifetimeMillis = 2000) {
    parent.AddChild(new FloatingLabel {
      Text = text,
      Position = position,
      Height = height,
      Color = color,
      FontSize = fontSize ?? 40,
      LifetimeMillis = lifetimeMillis,
    });
  }

  public static void FisherYatesShuffle<T>(List<T> list) {
    if (list.Count < 2) return;

    for (var i = list.Count - 1; i > 0; i--) {
      var j = Global.Random.Next(0, i);
      (list[i], list[j]) = (list[j], list[i]);
    }
  }

  public static Direction VectorToDirection(Vector2 vector) {
    if (MathF.Abs(vector.X) > MathF.Abs(vector.Y)) {
      return vector.X > 0 ? Direction.Right : Direction.Left;
    }
    return vector.Y > 0 ? Direction.Down : Direction.Up;
  }

  public static List<Card> GenerateLoot(int amount, Dictionary<Type, int> dropRate) {
    List<Card> loot = new();
    
    var total = 0;
    foreach (var entry in dropRate) {
      if (typeof(Card).IsAssignableFrom(entry.Key)) total += entry.Value;
      else dropRate.Remove(entry.Key);
    }
    
    for (var i = 0; i < amount; i++) {
      var selection = Global.Random.Next(total);
      var current = 0;
      foreach (var entry in dropRate) {
        current += entry.Value;
        if (selection >= current) continue;
        var card = (Card)Activator.CreateInstance(entry.Key)!;
        card.Protected = Global.Random.Next(100) == 0;
        loot.Add(card);
        break;
      }
    }
    
    return loot;
  }
  
  public static void SortCards(List<Card> cards) {
    cards.Sort((card, other) => {
      var nameOrder = string.Compare(card.Name, other.Name, StringComparison.Ordinal);
      if (nameOrder != 0) return nameOrder;
      var levelOrder = card.Level.CompareTo(other.Level);
      if (levelOrder != 0) return levelOrder;
      var protectionOrder = card.Protected.CompareTo(other.Protected);
      if (protectionOrder != 0) return -protectionOrder;
      return card.Origin.CompareTo(card.Origin);
    });
    return;
    GD.Print("Sorting result:");
    foreach (var card in cards) GD.Print($"  {card.Name} (lvl. {card.Level}) - {card.Origin}");
  }

  public static T GetRandomItem<T>(List<T> list) => list[Global.Random.Next(list.Count)];

  public static int GetMinutesBetween(DateTime start, DateTime end) => (int)(end - start).TotalMinutes;
}