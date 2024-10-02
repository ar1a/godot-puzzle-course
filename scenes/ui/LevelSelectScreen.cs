using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
    [Export]
    private PackedScene levelSelectSectionScene;
    private GridContainer gridContainer;

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("%GridContainer");

        var levelDefinitions = LevelManager.GetLevelDefinitions();
        for (var i = 0; i < levelDefinitions.Length; i++)
        {
            var levelSelectSection = levelSelectSectionScene.Instantiate<LevelSelectSection>();
            gridContainer.AddChild(levelSelectSection);

            levelSelectSection.SetLevelDefinition(levelDefinitions[i]);
            levelSelectSection.SetLevelIndex(i);
            levelSelectSection.LevelSelected += (index) => LevelManager.Instance.ChangeLevel(index);
        }
    }
}
