using System;
using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class MainMenu : Node
{
    [Export]
    private PackedScene optionsScene;

    private Button playButton;
    private Button optionsButton;
    private Button quitButton;
    private Control mainMenuContainer;
    private LevelSelectScreen levelSelectScreen;

    public override void _Ready()
    {
        mainMenuContainer = GetNode<Control>("%MainMenuContainer");
        levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");
        playButton = GetNode<Button>("%PlayButton");
        optionsButton = GetNode<Button>("%OptionsButton");
        quitButton = GetNode<Button>("%QuitButton");
        AudioHelpers.RegisterButtons(new Button[] { playButton, optionsButton, quitButton });

        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;

        playButton.Pressed += OnPlayButtonPressed;
        levelSelectScreen.BackPressed += OnLevelSelectBackPressed;
        quitButton.Pressed += () => GetTree().Quit();
        optionsButton.Pressed += OnOptionsButtonPressed;
    }

    private void OnOptionsButtonPressed()
    {
        mainMenuContainer.Visible = false;
        var optionsMenu = optionsScene.Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.DoneButtonPressed += () =>
        {
            optionsMenu.QueueFree();
            mainMenuContainer.Visible = true;
        };
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
