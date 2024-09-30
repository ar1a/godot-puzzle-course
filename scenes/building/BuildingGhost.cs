using Godot;

namespace Game.Building;

public partial class BuildingGhost : Node2D
{
    public override void _Ready() { }

    public void SetInvalid()
    {
        Modulate = Colors.Red;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
    }
}
