using Godot;

namespace Cardium.Scripts;

public partial class TileAlignedGameObject : AnimatedSprite2D
{
    public new Vector2I Position { get; set; }
    private Vector2I _previousPosition;
    
    public delegate void OnMoveDelegate(TileAlignedGameObject gameGameObject);
    public event OnMoveDelegate OnMoveEvent;
    
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
            OnMoveEvent?.Invoke(this);
            _previousPosition = Position;
        }
    }
    
    public void SetPosition(Vector2I position)
    {
        Position = position;
        base.Position = position * Global.TileSize;
    }

    protected void SetTexture(Texture2D texture)
    {
        // Create a new SpriteFrames resource
        var frames = new SpriteFrames();
        frames.AddAnimation("static");
        frames.SetAnimationLoop("static", false);

        // Add a single frame
        frames.AddFrame("static", texture);

        // Assign the SpriteFrames to the AnimatedSprite2D
        SpriteFrames = frames;
        Play("static", 0); // Not necessary, but ensures it stays on this frame
    }

    protected void SetAnimation(Texture2D spriteSheet, int frames, double fps)
    {
        var spriteFrames = new SpriteFrames();
        spriteFrames.AddAnimation("loop"); // Animation name
        spriteFrames.SetAnimationLoop("loop", true);
        spriteFrames.SetAnimationSpeed("loop", fps);

        var frameSize = Global.SpriteSize;

        for (var i = 0; i < frames; i++) // 4 frames
        {
            var region = new Rect2I(i * frameSize.X, 0, frameSize.X, frameSize.Y);
            
            var image = spriteSheet.GetImage();
            var frameImage = image.GetRegion(region);
            Texture2D frameTexture = ImageTexture.CreateFromImage(frameImage);

            spriteFrames.AddFrame("loop", frameTexture);
        }

        SpriteFrames = spriteFrames;
        
        Play("loop");
    }
}