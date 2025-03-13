using Godot;

namespace Cardium.Scripts;

public class Utils
{
    public static int ManhattanDistanceBetween(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
}