using Game.Autoload;
using Godot;

namespace Game.UI;

public partial class OptionsMenu : CanvasLayer
{
    private const string SFX_BUS = "SFX";
    private const string MUSIC_BUS = "Music";

    [Signal]
    public delegate void DoneButtonPressedEventHandler();

    private Button sfxUpButton;
    private Button sfxDownButton;
    private Label sfxLabel;

    private Button musicUpButton;
    private Button musicDownButton;
    private Label musicLabel;

    private Button windowButton;
    private Button doneButton;

    public override void _Ready()
    {
        sfxUpButton = GetNode<Button>("%SFXUpButton");
        sfxDownButton = GetNode<Button>("%SFXDownButton");
        sfxLabel = GetNode<Label>("%SFXLabel");

        musicUpButton = GetNode<Button>("%MusicUpButton");
        musicDownButton = GetNode<Button>("%MusicDownButton");
        musicLabel = GetNode<Label>("%MusicLabel");

        windowButton = GetNode<Button>("%WindowButton");
        doneButton = GetNode<Button>("%DoneButton");

        AudioHelpers.RegisterButtons(
            new Button[]
            {
                sfxUpButton,
                sfxDownButton,
                musicUpButton,
                musicDownButton,
                windowButton,
                doneButton,
            }
        );
        UpdateDisplay();

        sfxUpButton.Pressed += () => ChangeBusVolume(SFX_BUS, .1f);
        sfxDownButton.Pressed += () => ChangeBusVolume(SFX_BUS, -.1f);
        musicUpButton.Pressed += () => ChangeBusVolume(MUSIC_BUS, .1f);
        musicDownButton.Pressed += () => ChangeBusVolume(MUSIC_BUS, -.1f);
        windowButton.Pressed += () =>
        {
            OptionsHelper.ToggleWindowMode();
            UpdateDisplay();
        };
        doneButton.Pressed += () => EmitSignal(SignalName.DoneButtonPressed);
    }

    private void UpdateDisplay()
    {
        sfxLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumePercent(SFX_BUS) * 10).ToString();
        musicLabel.Text = Mathf.Round(OptionsHelper.GetBusVolumePercent(MUSIC_BUS) * 10).ToString();
        windowButton.Text = OptionsHelper.IsFullscreen() ? "Fullscreen" : "Windowed";
    }

    private void ChangeBusVolume(string busName, float change)
    {
        var busVolumePercent = OptionsHelper.GetBusVolumePercent(busName);
        busVolumePercent = Mathf.Clamp(busVolumePercent + change, 0, 1);
        OptionsHelper.SetBusVolumePercent(busName, busVolumePercent);
        UpdateDisplay();
    }
}
