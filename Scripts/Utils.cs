using System;
using System.Collections.Generic;
using Cardium.Scripts.Labels;
using Godot;

namespace Cardium.Scripts;

public class Utils
{
    public static int ManhattanDistanceBetween(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
    
    public static void SpawnFallingLabel(SceneTree tree, Vector2 position, string text, Color? color = null, int? fontSize = null, int? lifetimeMillis = null) {
        tree.Root.AddChild(new FallingLabel {
            Text = text,
            Position = position,
            Color = color,
            FontSize = fontSize,
            LifetimeMillis = lifetimeMillis,
        });
    }
    
    public static void SpawnFloatingLabel(SceneTree tree, Vector2 position, string text, Color? color = null, int height = 120, int? fontSize = null, int? lifetimeMillis = 2000) {
        tree.Root.AddChild(new FloatingLabel {
            Text = text,
            Position = position,
            Height = height,
            Color = color,
            FontSize = fontSize ?? 40,
            LifetimeMillis = lifetimeMillis,
        });
    }
    
    public static void FisherYatesShuffle<T>(List<T> list)
    {
        if (list.Count < 2) return;
        
        var random = new Random();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}