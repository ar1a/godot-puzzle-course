using Game.Autoload;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
    [Export]
    public int BuildableRadius { get; private set; }

    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
        Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
    }

    public Vector2I GetGridCellPosition()
    {
        return (Vector2I)(GlobalPosition / 64).Floor();
    }
}
