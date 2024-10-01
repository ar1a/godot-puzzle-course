using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{
    [Signal]
    public delegate void PressedEventHandler();

    private Label label;
    private Button button;

    public override void _Ready()
    {
        label = GetNode<Label>("%Label");
        button = GetNode<Button>("%Button");

        button.Pressed += () => EmitSignal(SignalName.Pressed);
    }

    public void SetBuildingResource(BuildingResource buildingResource)
    {
        label.Text = buildingResource.DisplayName;
        button.Text = $"Select (Cost {buildingResource.ResourceCost})";
    }
}
