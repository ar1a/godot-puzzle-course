using Game.Manager;
using Game.UI;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    [Export]
    private PackedScene levelCompleteScene;

    private GridManager gridManager;
    private GoldMine goldMine;
    private GameCamera gameCamera;
    private Node2D baseBuilding;
    private TileMapLayer baseTerrainTilemapLayer;
    private GameUI gameUI;

    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gameCamera = GetNode<GameCamera>("GameCamera");
        baseTerrainTilemapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        baseBuilding = GetNode<Node2D>("%Base");
        gameUI = GetNode<GameUI>("GameUI");

        gameCamera.SetBoundingRect(baseTerrainTilemapLayer.GetUsedRect());
        gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );
        if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition))
        {
            goldMine.SetActive();
            var levelCompleteScreen = levelCompleteScene.Instantiate<LevelCompleteScreen>();
            AddChild(levelCompleteScreen);
            gameUI.HideUI();
        }
    }
}
