using Godot;

namespace Game.Autoload;

public partial class OptionsHelper : Node
{
    public static void SetBusVolumePercent(string busName, float volumePercent)
    {
        var busIndex = AudioServer.GetBusIndex(busName);
        AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volumePercent));
    }

    public static float GetBusVolumePercent(string busName)
    {
        var busIndex = AudioServer.GetBusIndex(busName);
        return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
    }

    public static void ToggleWindowMode()
    {
        DisplayServer.WindowSetMode(
            IsFullscreen()
                ? DisplayServer.WindowMode.Windowed
                : DisplayServer.WindowMode.ExclusiveFullscreen
        );
    }

    public static bool IsFullscreen()
    {
        return DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen;
    }
}
