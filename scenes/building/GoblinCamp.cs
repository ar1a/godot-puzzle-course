using System;
using Game.Component;
using Godot;

namespace Game.Building;

public partial class GoblinCamp : Node2D
{
    [Export]
    private BuildingComponent buildingComponent;

    [Export]
    private AnimatedSprite2D fire;

    [Export]
    private AnimatedSprite2D animatedSprite2D;

    public override void _Ready()
    {
        fire.Visible = false;
        buildingComponent.Disabled += OnDisabled;
        buildingComponent.Enabled += OnEnabled;
    }

    private void OnDisabled()
    {
        animatedSprite2D.Play("destroyed");
        fire.Visible = true;
    }

    private void OnEnabled()
    {
        animatedSprite2D.Play("default");
        fire.Visible = false;
    }
}
