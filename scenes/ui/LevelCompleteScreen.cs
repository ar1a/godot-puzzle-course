using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
    [Export(PropertyHint.File, "*.tscn")]
    private string mainMenuScenePath;

    private Button nextLevelButton;

    public override void _Ready()
    {
        nextLevelButton = GetNode<Button>("%NextLevelButton");
        AudioHelpers.PlayVictory();

        if (LevelManager.IsLastLevel())
        {
            nextLevelButton.Text = "Return to Menu";
        }

        nextLevelButton.Pressed += OnNextLevelButtonPressed;
    }

    private void OnNextLevelButtonPressed()
    {
        if (!LevelManager.IsLastLevel())
        {
            LevelManager.ChangeToNextLevel();
        }
        else
        {
            GetTree().ChangeSceneToFile(mainMenuScenePath);
        }
    }
}
