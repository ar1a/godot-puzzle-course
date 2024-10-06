using Game.Autoload;
using Game.Manager;
using Game.Resources.Level;
using Game.UI;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    private readonly StringName PAUSE_ACTION = "pause";

    [Export]
    private PackedScene levelCompleteScene;

    [Export]
    private PackedScene escapeMenuScene;

    [Export]
    private LevelDefinitionResource levelDefinitionResource;

    private GridManager gridManager;
    private BuildingManager buildingManager;
    private GoldMine goldMine;
    private GameCamera gameCamera;
    private Node2D baseBuilding;
    private TileMapLayer baseTerrainTilemapLayer;
    private GameUI gameUI;
    private bool isComplete;

    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        buildingManager = GetNode<BuildingManager>("BuildingManager");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gameCamera = GetNode<GameCamera>("GameCamera");
        baseTerrainTilemapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        baseBuilding = GetNode<Node2D>("%Base");
        gameUI = GetNode<GameUI>("GameUI");

        buildingManager.SetStartingResourceCount(levelDefinitionResource.StartingResourceCount);

        gameCamera.SetBoundingRect(baseTerrainTilemapLayer.GetUsedRect());
        gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

        gridManager.SetGoldMinePosition(
            GridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition)
        );

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(PAUSE_ACTION))
        {
            GetTree().Paused = true;
            var escapeMenu = escapeMenuScene.Instantiate<EscapeMenu>();
            AddChild(escapeMenu);
            GetViewport().SetInputAsHandled();
        }
    }

    private void ShowLevelComplete()
    {
        isComplete = true;
        SaveManager.SaveLevelCompletion(levelDefinitionResource);
        goldMine.SetActive();
        var levelCompleteScreen = levelCompleteScene.Instantiate<LevelCompleteScreen>();
        AddChild(levelCompleteScreen);
        gameUI.HideUI();
    }

    private void OnGridStateUpdated()
    {
        if (isComplete)
            return;
        var goldMineTilePosition = GridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );
        if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition))
            ShowLevelComplete();
    }
}
