using System;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

    [Export]
    private BuildingResource[] buildingResources;

    [Export]
    private PackedScene buildingSectionScene;

    [Export]
    private BuildingManager buildingManager;

    private VBoxContainer bulidingSectionContainer;
    private Label resourceLabel;

    public override void _Ready()
    {
        bulidingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
        resourceLabel = GetNode<Label>("%ResourceLabel");
        CreateBuildingSections();

        buildingManager.AvailableResourceCountChanged += OnAvailableResourceCountChanged;
    }

    public void HideUI()
    {
        Visible = false;
    }

    private void CreateBuildingSections()
    {
        foreach (var buildingResource in buildingResources)
        {
            var buildingButton = buildingSectionScene.Instantiate<BuildingSection>();
            bulidingSectionContainer.AddChild(buildingButton);
            buildingButton.SetBuildingResource(buildingResource);
            buildingButton.Pressed += () =>
                EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
        }
    }

    private void OnAvailableResourceCountChanged(int newResourceCount)
    {
        resourceLabel.Text = newResourceCount.ToString();
    }
}
