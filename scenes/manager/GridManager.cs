using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Game.Util;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
    private const string IS_BUILDABLE = "is_buildable";
    private const string IS_IGNORED = "is_ignored";
    private const string IS_WOOD = "is_wood";

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

    [Signal]
    public delegate void GridStateUpdatedEventHandler();

    private HashSet<Vector2I> validBuildableTiles = new();
    private HashSet<Vector2I> collectedResourceTiles = new();
    private HashSet<Vector2I> occupiedTiles = new();

    [Export]
    private TileMapLayer highlightTilemapLayer;

    [Export]
    private TileMapLayer baseTerrainTilemapLayer;

    private List<TileMapLayer> allTilemapLayers = new();
    private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
        allTilemapLayers = GetAllTilemapLayers(baseTerrainTilemapLayer);
        MapTileMapLayersToElevationLayers();
    }

    public (TileMapLayer, bool) GetTileCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in allTilemapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData == null || (bool)customData.GetCustomData(IS_IGNORED))
                continue;
            return (layer, (bool)customData.GetCustomData(dataName));
        }
        return (null, false);
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
    }

    public bool IsTileAreaBuildable(Rect2I tileArea)
    {
        var tiles = tileArea.ToTiles();
        if (tiles.Count == 0)
            return false;

        (TileMapLayer firstTileMapLayer, _) = GetTileCustomData(tiles[0], IS_BUILDABLE);
        var targetElevationLayer = tileMapLayerToElevationLayer[firstTileMapLayer];

        return tiles.All(
            (tilePosition) =>
            {
                (TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(
                    tilePosition,
                    IS_BUILDABLE
                );
                var elevationLayer = tileMapLayerToElevationLayer[tileMapLayer];
                return isBuildable
                    && validBuildableTiles.Contains(tilePosition)
                    && elevationLayer == targetElevationLayer;
            }
        );
    }

    public void HighlightBuildableTiles()
    {
        foreach (var tilePosition in validBuildableTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }

    public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
    {
        var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles);
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightResourceTiles(Rect2I tileArea, int radius)
    {
        var resourceTiles = GetResourceTilesInRadius(tileArea, radius);
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in resourceTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPosition()
    {
        return ConvertWorldPositionToTilePosition(highlightTilemapLayer.GetGlobalMousePosition());
    }

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        return (Vector2I)(worldPosition / 64).Floor();
    }

    private List<TileMapLayer> GetAllTilemapLayers(Node2D rootNode)
    {
        var result = new List<TileMapLayer>();
        var children = rootNode.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is Node2D childNode)
            {
                result.AddRange(GetAllTilemapLayers(childNode));
            }
        }
        if (rootNode is TileMapLayer tileMapLayer)
        {
            result.Add(tileMapLayer);
        }
        return result;
    }

    private void MapTileMapLayersToElevationLayers()
    {
        foreach (var layer in allTilemapLayers)
        {
            ElevationLayer elevationLayer;
            Node startNode = layer;
            do
            {
                var parent = startNode.GetParent();
                elevationLayer = parent as ElevationLayer;
                startNode = parent;
            } while (elevationLayer == null && startNode != null);

            tileMapLayerToElevationLayer[layer] = elevationLayer;
        }
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());
        var rootCell = buildingComponent.GetGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
        var validTiles = GetValidTilesInRadius(
            tileArea,
            buildingComponent.BuildingResource.BuildableRadius
        );
        validBuildableTiles.UnionWith(validTiles);
        validBuildableTiles.ExceptWith(occupiedTiles);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
        var resourceTiles = GetResourceTilesInRadius(
            tileArea,
            buildingComponent.BuildingResource.ResourceRadius
        );
        var oldResourceTileCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(resourceTiles);
        if (oldResourceTileCount != collectedResourceTiles.Count)
        {
            EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
        }
        EmitSignal(SignalName.GridStateUpdated);
    }

    private void RecalculateGrid(BuildingComponent excludeBuildingComponent)
    {
        validBuildableTiles.Clear();
        occupiedTiles.Clear();
        collectedResourceTiles.Clear();

        var buildingComponents = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>()
            .Where((buildingComponent) => buildingComponent != excludeBuildingComponent);
        foreach (var buildingComponent in buildingComponents)
        {
            UpdateValidBuildableTiles(buildingComponent);
            UpdateCollectedResourceTiles(buildingComponent);
        }
        EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosiiton, float radius)
    {
        var dx = centerPosition.X - (tilePosiiton.X + .5);
        var dy = centerPosition.Y - (tilePosiiton.Y + .5);
        var distance = (dx * dx) + (dy * dy);
        return distance <= radius * radius;
    }

    private List<Vector2I> GetTilesInRadius(
        Rect2I tileArea,
        int radius,
        Func<Vector2I, bool> filterFn
    )
    {
        var result = new List<Vector2I>();
        var tileAreaCenter = tileArea.ToRect2F().GetCenter();
        var radiusMod = Mathf.Max(tileArea.Size.X, tileArea.Size.Y) / 2;
        for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
        {
            for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (
                    !IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod)
                    || !filterFn(tilePosition)
                )
                    continue;
                result.Add(tilePosition);
            }
        }
        return result;
    }

    private List<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(
            tileArea,
            radius,
            (tilePosition) => GetTileCustomData(tilePosition, IS_BUILDABLE).Item2
        );
    }

    private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(
            tileArea,
            radius,
            (tilePosition) => GetTileCustomData(tilePosition, IS_WOOD).Item2
        );
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
        UpdateCollectedResourceTiles(buildingComponent);
    }

    private void OnBuildingDestroyed(BuildingComponent buildingComponent)
    {
        RecalculateGrid(buildingComponent);
    }
}
