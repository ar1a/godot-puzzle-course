using System;
using Game.Autoload;
using Godot;

public partial class MainMenu : Node
{
    private Button playButton;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");

        playButton.Pressed += OnPlayButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        LevelManager.Instance.ChangeLevel(0);
    }
}
