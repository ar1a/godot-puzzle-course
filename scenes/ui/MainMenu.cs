using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class MainMenu : Node
{
    private Button playButton;
    private Button quitButton;
    private Control mainMenuContainer;
    private LevelSelectScreen levelSelectScreen;

    public override void _Ready()
    {
        mainMenuContainer = GetNode<Control>("%MainMenuContainer");
        levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");
        playButton = GetNode<Button>("%PlayButton");
        quitButton = GetNode<Button>("%QuitButton");
        AudioHelpers.RegisterButtons(new Button[] { playButton, quitButton });

        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;

        playButton.Pressed += OnPlayButtonPressed;
        levelSelectScreen.BackPressed += OnLevelSelectBackPressed;
        quitButton.Pressed += () => GetTree().Quit();
    }

    private void OnLevelSelectBackPressed()
    {
        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;
    }

    private void OnPlayButtonPressed()
    {
        mainMenuContainer.Visible = false;
        levelSelectScreen.Visible = true;
    }
}
