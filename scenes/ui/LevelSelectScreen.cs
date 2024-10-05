using System;
using Game.Autoload;
using Game.Resources.Level;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
    private const int PAGE_SIZE = 6;

    [Signal]
    public delegate void BackPressedEventHandler();

    [Export]
    private PackedScene levelSelectSectionScene;
    private GridContainer gridContainer;

    private Button backButton;
    private Button previousPageButton;
    private Button nextPageButton;

    private int pageIndex;
    private int maxPageIndex;
    private LevelDefinitionResource[] levelDefinitions;

    //TODO: delete me?
    private int startIndex => PAGE_SIZE * pageIndex;
    private int endIndex => Mathf.Min(startIndex + PAGE_SIZE, levelDefinitions.Length);

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("%GridContainer");
        backButton = GetNode<Button>("BackButton");
        previousPageButton = GetNode<Button>("%PreviousPageButton");
        nextPageButton = GetNode<Button>("%NextPageButton");

        levelDefinitions = LevelManager.GetLevelDefinitions();
        maxPageIndex = levelDefinitions.Length / PAGE_SIZE;

        backButton.Pressed += () => EmitSignal(SignalName.BackPressed);
        previousPageButton.Pressed += () => OnPageChanged(-1);
        nextPageButton.Pressed += () => OnPageChanged(1);

        ShowPage();
    }

    private void OnPageChanged(int change)
    {
        pageIndex += change;
        ShowPage();
    }

    private void UpdateButtonVisibility()
    {
        previousPageButton.Disabled = pageIndex == 0;
        previousPageButton.Modulate = previousPageButton.Disabled
            ? Colors.Transparent
            : Colors.White;
        nextPageButton.Disabled = pageIndex == maxPageIndex;
        nextPageButton.Modulate = nextPageButton.Disabled ? Colors.Transparent : Colors.White;
    }

    private void ShowPage()
    {
        UpdateButtonVisibility();

        foreach (var child in gridContainer.GetChildren())
        {
            child.QueueFree();
        }

        var startIndex = PAGE_SIZE * pageIndex;
        var endIndex = Mathf.Min(startIndex + PAGE_SIZE, levelDefinitions.Length);
        for (var i = startIndex; i < endIndex; i++)
        {
            var levelSelectSection = levelSelectSectionScene.Instantiate<LevelSelectSection>();
            gridContainer.AddChild(levelSelectSection);

            levelSelectSection.SetLevelDefinition(levelDefinitions[i]);
            levelSelectSection.SetLevelIndex(i);
            levelSelectSection.LevelSelected += static (index) =>
                LevelManager.Instance.ChangeLevel(index);
        }
    }
}
