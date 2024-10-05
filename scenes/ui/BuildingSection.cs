using Game.Autoload;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{
    [Signal]
    public delegate void PressedEventHandler();

    private Label titleLabel;
    private Label descriptionLabel;
    private Label costLabel;
    private Button button;

    public override void _Ready()
    {
        titleLabel = GetNode<Label>("%TitleLabel");
        descriptionLabel = GetNode<Label>("%DescriptionLabel");
        costLabel = GetNode<Label>("%CostLabel");
        button = GetNode<Button>("%Button");

        button.Pressed += () => EmitSignal(SignalName.Pressed);
        AudioHelpers.RegisterButtons(new Button[] { button });
    }

    public void SetBuildingResource(BuildingResource buildingResource)
    {
        titleLabel.Text = buildingResource.DisplayName;
        costLabel.Text = $"{buildingResource.ResourceCost}";
        descriptionLabel.Text = buildingResource.Description;
    }
}
