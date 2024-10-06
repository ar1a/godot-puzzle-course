using System.Linq;
using Game.Resources.Level;
using Godot;

namespace Game.Autoload;

public partial class LevelManager : Node
{
    private static LevelManager instance;

    [Export]
    private LevelDefinitionResource[] levelDefinitions;

    private static int currentLevelIndex;

    public static void ChangeLevel(int levelIndex)
    {
        if (levelIndex >= instance.levelDefinitions.Length || levelIndex < 0)
        {
            return;
        }
        currentLevelIndex = levelIndex;
        var levelDefinition = instance.levelDefinitions[currentLevelIndex];
        instance.GetTree().ChangeSceneToFile(levelDefinition.LevelScenePath);
    }

    public static LevelDefinitionResource[] GetLevelDefinitions()
    {
        return instance.levelDefinitions.ToArray();
    }

    public static void ChangeToNextLevel()
    {
        ChangeLevel(currentLevelIndex + 1);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            instance = this;
        }
    }

    public static bool IsLastLevel()
    {
        return currentLevelIndex == instance.levelDefinitions.Length - 1;
    }
}
