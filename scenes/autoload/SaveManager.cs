using System;
using Game.Resources.Level;
using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE_PATH = "user://save.json";
    private static SaveData saveData = new();

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
            LoadSaveData();
            GD.Print(saveData.LevelCompletionStatus.Keys.Count);
        }
    }

    public static bool IsLevelCompleted(string levelId)
    {
        saveData.LevelCompletionStatus.TryGetValue(levelId, out var data);
        return data?.IsCompleted == true;
    }

    public static void SaveLevelCompletion(LevelDefinitionResource levelDefinitionResource)
    {
        saveData.SaveLevelCompletion(levelDefinitionResource.Id, true);
        WriteSaveData();
    }

    private static void WriteSaveData()
    {
        var dataString = JsonConvert.SerializeObject(saveData);
        using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Write);
        saveFile.StoreLine(dataString);
    }

    private static void LoadSaveData()
    {
        if (!FileAccess.FileExists(SAVE_FILE_PATH))
            return;

        using var saveFile = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
        var dataString = saveFile.GetLine();
        try
        {
            saveData = JsonConvert.DeserializeObject<SaveData>(dataString);
        }
        catch (Exception)
        {
            GD.PushWarning("Save file json was corrupted");
        }
    }
}
