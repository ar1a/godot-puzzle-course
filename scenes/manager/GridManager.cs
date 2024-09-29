using System.Collections.Generic;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
    private HashSet<Vector2> occupiedCells = new();

    [Export]
    private TileMapLayer highlightTilemapLayer;

    [Export]
    private TileMapLayer baseTerrainTilemapLayer;

    public override void _Ready() { }

    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        occupiedCells.Add(tilePosition);
    }

    public bool IsTilePositionValid(Vector2 tilePosition)
    {
        return !occupiedCells.Contains(tilePosition);
    }

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        ClearHighlightedTiles();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                if (!IsTilePositionValid(new Vector2(x, y)))
                    continue;
                highlightTilemapLayer.SetCell(new Vector2I((int)x, (int)y), 1, Vector2I.Zero);
            }
        }
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public Vector2 GetMouseGridCellPosition()
    {
        return (highlightTilemapLayer.GetGlobalMousePosition() / 64).Floor();
    }
}
