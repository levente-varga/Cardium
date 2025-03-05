using System.Threading.Tasks;
using Godot;

namespace Cardium.Scripts;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public partial class TileAlignedGameObject : AnimatedSprite2D
{
    public new Vector2I Position { get; set; }
    private Vector2I _previousPosition;
    
    public delegate void OnMoveDelegate(TileAlignedGameObject source, Vector2I oldPosition, Vector2I newPosition);
    public event OnMoveDelegate OnMoveEvent;
    
    public delegate void OnNudgeDelegate(TileAlignedGameObject source, Vector2I position);
    public event OnNudgeDelegate OnNudgeEvent;

    public override void _Ready()
    {
        Scale = new Vector2(Global.Scale, Global.Scale);
        Centered = false;
    }

    public override void _Process(double delta)
    {
        if (base.Position.DistanceTo(Position * Global.TileSize) < 1)
        {
            base.Position = Position * Global.TileSize;
        }
        else
        {
            base.Position = base.Position.Lerp(Position * Global.TileSize, Global.LerpWeight * (float) delta);
        }

        if (_previousPosition != Position)
        {
            OnMoveEvent?.Invoke(this, _previousPosition, Position);
            _previousPosition = Position;
        }
    }
    
    public void SetPosition(Vector2I position)
    {
        Position = position;
        base.Position = position * Global.TileSize;
    }

    protected void SetStillFrame(Texture2D texture)
    {
        // Create a new SpriteFrames resource
        var frames = new SpriteFrames();
        frames.AddAnimation("still");
        frames.SetAnimationLoop("still", false);

        // Add a single frame
        frames.AddFrame("still", texture);

        // Assign the SpriteFrames to the AnimatedSprite2D
        SpriteFrames = frames;
        Play("still", 0); // Not necessary, but ensures it stays on this frame
    }

    protected void SetAnimation(string name, Texture2D spriteSheet, int frames, double fps, bool autoPlay = true, bool loop = true)
    {
        var spriteFrames = new SpriteFrames();
        spriteFrames.AddAnimation(name); // Animation name
        spriteFrames.SetAnimationLoop(name, loop);
        spriteFrames.SetAnimationSpeed(name, fps);

        var frameSize = Global.SpriteSize;

        for (var i = 0; i < frames; i++) // 4 frames
        {
            var region = new Rect2I(i * frameSize.X, 0, frameSize.X, frameSize.Y);
            
            var image = spriteSheet.GetImage();
            var frameImage = image.GetRegion(region);
            Texture2D frameTexture = ImageTexture.CreateFromImage(frameImage);

            spriteFrames.AddFrame(name, frameTexture);
        }

        SpriteFrames = spriteFrames;
        Animation = name;
        
        if (autoPlay) Play(name);
    }

    protected static async Task Delay(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
    
    protected static async Task DebugDelay(int milliseconds)
    {
        if (Global.Debug) await Delay(milliseconds);
    }
    
    protected void Nudge(Direction direction)
    {
        base.Position += DirectionToVector(direction) * Global.TileSize / 8;
        OnNudgeEvent?.Invoke(this, Position + DirectionToVector(direction));
    }

    public void Blink()
    {
        Tween tween = CreateTween();
        Modulate = new Color(0f, 0f, 0f, 1); // Set red
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
        tween.Play();
    }
    
    public void SpawnFallingLabel(string text, Color? color = null, int? fontSize = null, int? lifetimeMillis = null)
    {
        Labels.FallingLabel label = new()
        {
            Text = text,
            Position = GlobalPosition + Global.TileSize / 2,
            Color = color,
            FontSize = fontSize,
            LifetimeMillis = lifetimeMillis,
        };
        GetTree().Root.AddChild(label);
    }
    
    public void SpawnFloatingLabel(string text, Color? color = null, int? fontSize = null, int? lifetimeMillis = 2000)
    {
        Labels.FloatingLabel label = new()
        {
            Text = text,
            Position = GlobalPosition + Global.TileSize / 2,
            Color = color,
            FontSize = fontSize ?? 40,
            LifetimeMillis = lifetimeMillis,
        };
        GetTree().Root.AddChild(label);
    }

    protected void SpawnDebugFloatingLabel(string text)
    {
        if (Global.Debug) SpawnFloatingLabel("[Debug] " + text, color: Global.Magenta, fontSize: 20);
    }
    
    protected void SpawnDebugFallingLabel(string text)
    {
        if (Global.Debug) SpawnFallingLabel("[Debug] " + text, color: Global.Magenta, fontSize: 20);
    }
    
    protected static Vector2I DirectionToVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2I.Up,
            Direction.Down => Vector2I.Down,
            Direction.Left => Vector2I.Left,
            Direction.Right => Vector2I.Right,
            _ => Vector2I.Zero
        };
    }

    protected static Direction VectorToDirection(Vector2 vector)
    {
        var normalized = vector.Normalized();
        
        if (Mathf.Abs(normalized.X) > Mathf.Abs(normalized.Y))
            return normalized.X > 0 ? Direction.Right : Direction.Left;
        return normalized.Y > 0 ? Direction.Up : Direction.Down;
    }
}