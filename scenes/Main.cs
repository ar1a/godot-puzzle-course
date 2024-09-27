using System;
using Godot;

namespace Game;

public partial class Main : Node2D
{
    private Sprite2D cursor;
    private PackedScene buildingScene;
    private Button placeBuildingButton;
    private TileMapLayer highlightTileMapLayer;

    private Vector2? hoveredGridCell;

    // Called when the node enters the scene tree for the first time.
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
        if (evt.IsActionPressed("left_click") && cursor.Visible)
        {
            PlaceBuildingAtMousePosition();
            cursor.Visible = false;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
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
        return (GetGlobalMousePosition() / 64).Floor();
    }

    private void PlaceBuildingAtMousePosition()
    {
        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = GetMouseGridCellPosition() * 64;
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
