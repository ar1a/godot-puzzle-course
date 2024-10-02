using System.Linq;
using Game.Resources.Level;
using Godot;

namespace Game.Autoload;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export]
    private LevelDefinitionResource[] levelDefinitions;

    private int currentLevelIndex;

    public void ChangeLevel(int levelIndex)
    {
        if (levelIndex >= levelDefinitions.Length || levelIndex < 0)
        {
            return;
        }
        currentLevelIndex = levelIndex;
        var levelDefinition = levelDefinitions[currentLevelIndex];
        GetTree().ChangeSceneToFile(levelDefinition.LevelScenePath);
    }

    public static LevelDefinitionResource[] GetLevelDefinitions()
    {
        return Instance.levelDefinitions.ToArray();
    }

    public void ChangeToNextLevel()
    {
        ChangeLevel(currentLevelIndex + 1);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }
}
