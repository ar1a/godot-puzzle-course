using System.Collections.Generic;
using Godot;

namespace Game;

public partial class Main : Node
{
    private Sprite2D cursor;
    private PackedScene buildingScene;
    private Button placeBuildingButton;
    private TileMapLayer highlightTileMapLayer;

    private Vector2? hoveredGridCell;
    private HashSet<Vector2> occupiedCells = new();

    public override void _Ready()
    {
        cursor = GetNode<Sprite2D>("Cursor");
        buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

        cursor.Visible = false;

        placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (
            hoveredGridCell.HasValue
            && evt.IsActionPressed("left_click")
            && cursor.Visible
            && !occupiedCells.Contains(hoveredGridCell.Value)
        )
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            UpdateHighlightTileMapLayer();
        }
    }

    private Vector2 GetMouseGridCellPosition()
    {
        return (highlightTileMapLayer.GetGlobalMousePosition() / 64).Floor();
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue)
            return;
        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;
        occupiedCells.Add(hoveredGridCell.Value);
        hoveredGridCell = null;
        UpdateHighlightTileMapLayer();
    }

    private void UpdateHighlightTileMapLayer()
    {
        highlightTileMapLayer.Clear();
        if (!hoveredGridCell.HasValue)
        {
            return;
        }

        for (var x = hoveredGridCell.Value.X - 3; x <= hoveredGridCell.Value.X + 3; x++)
        {
            for (var y = hoveredGridCell.Value.Y - 3; y <= hoveredGridCell.Value.Y + 3; y++)
            {
                highlightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 1, Vector2I.Zero);
            }
        }
    }

    private void OnButtonPressed()
    {
        cursor.Visible = true;
    }
}
