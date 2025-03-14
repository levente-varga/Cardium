using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Overlay : Node2D
{
	public struct OverlayTile
	{
		public Vector2I Position;
		public Color Color;
	}
	
	private List<OverlayTile> _tiles = new();

	public List<OverlayTile> Tiles
	{
		get => _tiles;
		set
		{
			_tiles = new List<OverlayTile>(value);
			QueueRedraw();
		}
	}

	public override void _Ready()
	{
		Modulate = new Color(1, 1, 1, 0.3f);
	}

	public override void _Draw()
	{
		foreach (var tile in _tiles)
		{
			DrawRect(new Rect2(Global.TileToWorld(tile.Position), Global.TileSize), tile.Color);
		}
	}
}