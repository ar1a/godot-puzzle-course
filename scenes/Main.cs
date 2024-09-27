using System;
using Godot;

namespace Game;

public partial class Main : Node2D
{
    private Sprite2D sprite;
    private PackedScene buildingScene;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Cursor");
        buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt.IsActionPressed("left_click"))
        {
            PlaceBuildingAtMousePosition();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        var gridPosition = GetMouseGridCellPosition();
        sprite.GlobalPosition = gridPosition * 64;
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
    }
}
