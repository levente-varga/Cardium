using Godot;

namespace Cardium.Scripts.Labels;

public partial class LabelWithShadow : Node2D
{
    private Label _label = new ();
    private Label _shadow = new ();
    
    public string Text = "";
    public Color? Color;
    public int? FontSize = 40;
    
    public override void _Ready()
    {
        var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");

        ZIndex = 100;
        
        _label = new Label();
        _label.Text = Text;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", FontSize ?? 40);
        _label.Modulate = Color ?? new Color("FFFFFF");
        _label.ZIndex = 1;
        AddChild(_label);
        
        _shadow.Text = Text;
        _shadow.AddThemeFontOverride("font", font);
        _shadow.AddThemeFontSizeOverride("font_size", FontSize ?? 40);
        _shadow.Modulate = new Color(0, 0, 0, 1f);
        _shadow.Position = new Vector2(4, 4);
        _shadow.ZIndex = _label.ZIndex - 1;
        AddChild(_shadow);
    }

    public override void _Process(double delta)
    {
        
    }
}