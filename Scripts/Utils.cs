using Godot;

namespace Cardium.Scripts;

public class Utils
{
    public static int ManhattanDistanceBetween(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
    
    public static void SpawnFallingLabel(SceneTree tree, Vector2 position, string text, Color? color = null, int? fontSize = null, int? lifetimeMillis = null)
    {
        Labels.FallingLabel label = new()
        {
            Text = text,
            Position = position,
            Color = color,
            FontSize = fontSize,
            LifetimeMillis = lifetimeMillis,
        };
        tree.Root.AddChild(label);
    }
    
    public static void SpawnFloatingLabel(SceneTree tree, Vector2 position, string text, Color? color = null, int? fontSize = null, int? lifetimeMillis = 2000)
    {
        Labels.FloatingLabel label = new()
        {
            Text = text,
            Position = position,
            Color = color,
            FontSize = fontSize ?? 40,
            LifetimeMillis = lifetimeMillis,
        };
        tree.Root.AddChild(label);
    }
}