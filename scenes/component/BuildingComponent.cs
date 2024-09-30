using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Resources.Building;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
    [Export(PropertyHint.File, "*.tres")]
    private string buildingResourcePath;

    public BuildingResource BuildingResource { get; private set; }

    private HashSet<Vector2I> occupiedTiles = new();

    public override void _Ready()
    {
        if (buildingResourcePath != null)
        {
            BuildingResource = GD.Load<BuildingResource>(buildingResourcePath);
        }
        AddToGroup(nameof(BuildingComponent));
        Callable.From(Initialize).CallDeferred();
    }

    public Vector2I GetGridCellPosition()
    {
        return (Vector2I)(GlobalPosition / 64).Floor();
    }

    public HashSet<Vector2I> GetOccupiedCellPositions()
    {
        return occupiedTiles.ToHashSet();
    }

    public bool IsTileInBuildingArea(Vector2I tilePosition)
    {
        return occupiedTiles.Contains(tilePosition);
    }

    public void Destroy()
    {
        GameEvents.EmitBuildingDestroyed(this);
        Owner.QueueFree();
    }

    private void CalculateOccupiedCellPositions()
    {
        var gridPosition = GetGridCellPosition();
        for (int x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
        {
            for (int y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
            {
                occupiedTiles.Add(new Vector2I(x, y));
            }
        }
    }

    private void Initialize()
    {
        CalculateOccupiedCellPositions();
        GameEvents.EmitBuildingPlaced(this);
    }
}
