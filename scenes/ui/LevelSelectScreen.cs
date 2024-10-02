using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
    [Signal]
    public delegate void BackPressedEventHandler();

    [Export]
    private PackedScene levelSelectSectionScene;
    private GridContainer gridContainer;

    private Button backButton;

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("%GridContainer");
        backButton = GetNode<Button>("BackButton");

        var levelDefinitions = LevelManager.GetLevelDefinitions();
        for (var i = 0; i < levelDefinitions.Length; i++)
        {
            var levelSelectSection = levelSelectSectionScene.Instantiate<LevelSelectSection>();
            gridContainer.AddChild(levelSelectSection);

            levelSelectSection.SetLevelDefinition(levelDefinitions[i]);
            levelSelectSection.SetLevelIndex(i);
            levelSelectSection.LevelSelected += (index) => LevelManager.Instance.ChangeLevel(index);
        }
        backButton.Pressed += () => EmitSignal(SignalName.BackPressed);
    }
}
