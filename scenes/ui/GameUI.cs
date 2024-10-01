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

    private VBoxContainer bulidingSectionContainer;

    public override void _Ready()
    {
        bulidingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
        CreateBuildingSections();
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
}
