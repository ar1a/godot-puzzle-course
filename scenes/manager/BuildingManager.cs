using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
    [Export]
    private GridManager gridManager;

    [Export]
    private GameUI gameUI;

    [Export]
    private Node2D ySortRoot;

    [Export]
    private Sprite2D cursor;

    private int currentResourceCount;
    private int startingResourceCount = 4;
    private int currentlyUsedResourceCount;
    private BuildingResource toPlaceBuildingResource;

    private int AvailableResourceCount =>
        startingResourceCount + currentResourceCount - currentlyUsedResourceCount;
    private Vector2I? hoveredGridCell;

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        gameUI.BuildingResourceSelected += OnBuildingResourseSelected;
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (
            hoveredGridCell.HasValue
            && toPlaceBuildingResource != null
            && evt.IsActionPressed("left_click")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value)
            && AvailableResourceCount >= toPlaceBuildingResource.ResourceCost
        )
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (
            toPlaceBuildingResource != null
            && cursor.Visible
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition)
        )
        {
            hoveredGridCell = gridPosition;
            gridManager.ClearHighlightedTiles();
            gridManager.HighlightExpandedBuildableTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.BuildableRadius
            );
            gridManager.HighlightResourceTiles(
                hoveredGridCell.Value,
                toPlaceBuildingResource.ResourceRadius
            );
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue)
            return;
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;
        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();

        currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
        GD.Print(AvailableResourceCount);
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount = resourceCount;
        // __AUTO_GENERATED_PRINT_VAR_START__
        GD.Print($"BuildingManager#OnResourceTilesUpdated resourceCount: {resourceCount}"); // __AUTO_GENERATED_PRINT_VAR_END__
    }

    private void OnBuildingResourseSelected(BuildingResource buildingResource)
    {
        cursor.Visible = true;
        gridManager.HighlightBuildableTiles();
        toPlaceBuildingResource = buildingResource;
    }
}
