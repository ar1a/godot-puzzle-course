using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public override void _Ready()
    {
        var saveData = new SaveData();
        saveData.SaveLevelCompletion("random_id", true);
        var dataString = JsonConvert.SerializeObject(saveData);
        // __AUTO_GENERATED_PRINT_VAR_START__
        GD.Print($"SaveManager#_Ready dataString: {dataString}"); // __AUTO_GENERATED_PRINT_VAR_END__

        var data = JsonConvert.DeserializeObject<SaveData>(dataString);
        // __AUTO_GENERATED_PRINT_VAR_START__
        GD.Print($"SaveManager#_Ready data: {data}"); // __AUTO_GENERATED_PRINT_VAR_END__
    }
}
