using System.Collections.Generic;
using System.Linq;
using Game.Building;
using Game.Component;
using Game.Resources.Building;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class BuildingManager : Node
{
    private readonly StringName ACTION_LEFT_CLICK = "left_click";
    private readonly StringName ACTION_CANCEL = "cancel";
    private readonly StringName ACTION_RIGHT_CLICK = "right_click";

    [Signal]
    public delegate void AvailableResourceCountChangedEventHandler(int newResourceCount);

    [Export]
    private GridManager gridManager;

    [Export]
    private GameUI gameUI;

    [Export]
    private Node2D ySortRoot;

    [Export]
    private PackedScene buildingGhostScene;

    private enum State
    {
        Normal,
        PlacingBuilding,
    }

    private int currentResourceCount;
    private int currentlyUsedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
    private BuildingGhost buildingGhost;
    private Vector2 buildingGhostDimensions;
    private State currentState = State.Normal;
    private int startingResourceCount = 0;

    private int AvailableResourceCount =>
        startingResourceCount + currentResourceCount - currentlyUsedResourceCount;

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        gameUI.BuildingResourceSelected += OnBuildingResourseSelected;
        Callable
            .From(
                () => EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount)
            )
            .CallDeferred();
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        switch (currentState)
        {
            case State.Normal:
                if (evt.IsActionPressed(ACTION_RIGHT_CLICK))
                {
                    DestroyBuildAtHoveredCellPosition();
                }
                break;
            case State.PlacingBuilding:
                if (evt.IsActionPressed(ACTION_CANCEL))
                {
                    ChangeState(State.Normal);
                }
                else if (
                    toPlaceBuildingResource != null
                    && evt.IsActionPressed(ACTION_LEFT_CLICK)
                    && IsBuildingPlaceableAtArea(hoveredGridArea)
                )
                {
                    PlaceBuildingAtHoveredCellPosition();
                }
                break;
            default:
                break;
        }
    }

    public bool IsBuildingPlaceableAtArea(Rect2I tileArea)
    {
        var isAttackTiles = toPlaceBuildingResource.IsAttackBuilding();
        var allTilesBuildable = gridManager.IsTileAreaBuildable(tileArea, isAttackTiles);
        return allTilesBuildable && AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
    }

    public void SetStartingResourceCount(int count)
    {
        startingResourceCount = count;
    }

    public override void _Process(double delta)
    {
        Vector2I mouseGridPosition = Vector2I.Zero;
        var rootCell = hoveredGridArea.Position;

        switch (currentState)
        {
            case State.Normal:
                mouseGridPosition = gridManager.GetMouseGridCellPosition();
                break;
            case State.PlacingBuilding:
                mouseGridPosition = gridManager.GetMouseGridCellPositionWithDimensionOffset(
                    buildingGhostDimensions
                );
                buildingGhost.GlobalPosition = mouseGridPosition * 64;
                break;
        }

        if (rootCell != mouseGridPosition)
        {
            hoveredGridArea.Position = mouseGridPosition;
            UpdateHoveredGridArea();
        }
    }

    public void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();

        if (toPlaceBuildingResource.IsAttackBuilding())
        {
            gridManager.HighlightGoblinOccupiedTiles();
            gridManager.HighlightBuildableTiles(true);
        }
        else
        {
            gridManager.HighlightBuildableTiles();
            gridManager.HighlightGoblinOccupiedTiles();
        }
        if (IsBuildingPlaceableAtArea(hoveredGridArea))
        {
            if (toPlaceBuildingResource.IsAttackBuilding())
            {
                gridManager.HighlightAttackTiles(
                    hoveredGridArea,
                    toPlaceBuildingResource.AttackRadius
                );
            }
            else
            {
                gridManager.HighlightExpandedBuildableTiles(
                    hoveredGridArea,
                    toPlaceBuildingResource.BuildableRadius
                );
            }
            gridManager.HighlightResourceTiles(
                hoveredGridArea,
                toPlaceBuildingResource.ResourceRadius
            );
            buildingGhost.SetValid();
        }
        else
        {
            buildingGhost.SetInvalid();
        }

        buildingGhost.DoHoverAnimation();
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridArea.Position * 64;
        building.GetFirstNodeOfType<BuildingAnimatorComponent>()?.PlayInAnimation();
        currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
        ChangeState(State.Normal);
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }

    private void DestroyBuildAtHoveredCellPosition()
    {
        var buildingComponent = BuildingComponent
            .GetValidBuildingComponents(this)
            .FirstOrDefault(
                (buildingComponent) =>
                    buildingComponent.IsTileInBuildingArea(hoveredGridArea.Position)
                    && buildingComponent.BuildingResource.IsDeletable
            );
        if (buildingComponent == null)
            return;
        if (!gridManager.CanDestroyBuilidng(buildingComponent))
            return;

        currentlyUsedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
        buildingComponent.Destroy();
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }

    public void ClearBuildingGhost()
    {
        gridManager.ClearHighlightedTiles();
        if (IsInstanceValid(buildingGhost))
            buildingGhost.QueueFree();
        buildingGhost = null;
    }

    private void UpdateHoveredGridArea()
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                UpdateGridDisplay();
                break;
        }
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount = resourceCount;
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }

    private void ChangeState(State toState)
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                ClearBuildingGhost();
                toPlaceBuildingResource = null;
                break;
            default:
                break;
        }

        currentState = toState;
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
                ySortRoot.AddChild(buildingGhost);
                break;
            default:
                break;
        }
    }

    private void OnBuildingResourseSelected(BuildingResource buildingResource)
    {
        ChangeState(State.PlacingBuilding);
        hoveredGridArea.Size = buildingResource.Dimensions;
        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        buildingGhost.AddSpriteNode(buildingSprite);
        buildingGhostDimensions = buildingResource.Dimensions;
        buildingGhost.SetDimensions(buildingResource.Dimensions);
        toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }
}
