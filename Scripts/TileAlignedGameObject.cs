using Godot;

namespace Cardium.Scripts;

public partial class TileAlignedGameObject : AnimatedSprite2D
{
    public new Vector2I Position { get; set; }
    private Vector2I _previousPosition;
    
    public delegate void OnMoveDelegate(TileAlignedGameObject gameGameObject, Vector2I position);
    public event OnMoveDelegate OnMoveEvent;
    
    public delegate void OnNudgeDelegate(TileAlignedGameObject gameGameObject, Vector2I position);
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
            OnMoveEvent?.Invoke(this, Position);
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
    
    protected void Nudge(Vector2I direction)
    {
        base.Position += direction * Global.TileSize / 8;
        OnNudgeEvent?.Invoke(this, Position + direction);
    }

    public void Blink()
    {
        Tween tween = CreateTween();
        Modulate = new Color(0f, 0f, 0f, 1); // Set red
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
        tween.Play();
    }
    
    public void SpawnFloatingLabel(string text, Color color)
    {
        Labels.FallingLabel label = new()
        {
            Text = text,
            Position = GlobalPosition + Global.TileSize / 2,
            Color = color,
        };
        GetTree().Root.AddChild(label);
    }
}