using Godot;

namespace Cardium.Scripts;

public partial class Camera : Camera2D
{
	[Export] public Sprite2D Target { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var targetCenter = Target.GlobalPosition + ((Target.Texture.GetSize() * Target.Scale) / 2);
		
		// lerp to player position:
		Position = GlobalPosition.Lerp(targetCenter, 0.1f);
	}
}