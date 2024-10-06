using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class EscapeMenu : CanvasLayer
{
    private readonly StringName PAUSE_ACTION = "pause";

    [Export(PropertyHint.File, "*.tscn")]
    private string mainMenuScenePath;

    [Export]
    private PackedScene optionsScene;

    private Button quitButton;
    private Button resumeButton;
    private Button optionsButton;
    private MarginContainer marginContainer;

    public override void _Ready()
    {
        quitButton = GetNode<Button>("%QuitButton");
        resumeButton = GetNode<Button>("%ResumeButton");
        optionsButton = GetNode<Button>("%OptionsButton");
        marginContainer = GetNode<MarginContainer>("MarginContainer");

        AudioHelpers.RegisterButtons(new Button[] { quitButton, resumeButton, optionsButton });

        quitButton.Pressed += () =>
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile(mainMenuScenePath);
        };
        resumeButton.Pressed += CloseMenu;
        optionsButton.Pressed += OnOptionsButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(PAUSE_ACTION))
        {
            CloseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    private void CloseMenu()
    {
        GetTree().Paused = false;
        QueueFree();
    }

    private void OnOptionsButtonPressed()
    {
        marginContainer.Visible = false;
        var optionsMenu = optionsScene.Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.DoneButtonPressed += () =>
        {
            optionsMenu.QueueFree();
            marginContainer.Visible = true;
        };
    }
}
