using Game.Manager;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{
    private GridManager gridManager;
    private GoldMine goldMine;
    private GameCamera gameCamera;
    private TileMapLayer baseTerrainTilemapLayer;

    public override void _Ready()
    {
        gridManager = GetNode<GridManager>("GridManager");
        goldMine = GetNode<GoldMine>("%GoldMine");
        gameCamera = GetNode<GameCamera>("GameCamera");
        baseTerrainTilemapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");

        gameCamera.SetBoundingRect(baseTerrainTilemapLayer.GetUsedRect());

        gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(
            goldMine.GlobalPosition
        );
        if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
        {
            GD.Print("Win!");
            goldMine.SetActive();
        }
    }
}
