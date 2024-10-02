using Godot;

namespace Game.Autoload;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export]
    private PackedScene[] levelScenes;

    private int currentLevelIndex;

    public void ChangeLevel(int levelIndex)
    {
        if (levelIndex >= levelScenes.Length || levelIndex < 0)
        {
            return;
        }
        currentLevelIndex = levelIndex;
        var levelScene = levelScenes[currentLevelIndex];
        GetTree().ChangeSceneToPacked(levelScene);
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
