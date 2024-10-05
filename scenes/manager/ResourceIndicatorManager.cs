using System.Collections.Generic;
using System.Linq;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class ResourceIndicatorManager : Node
{
    [Export]
    private GridManager gridManager;

    [Export]
    private PackedScene resourceIndicatorScene;

    private AudioStreamPlayer audioStreamPlayer;

    private HashSet<Vector2I> indicatedTiles = new();
    private readonly Dictionary<Vector2I, ResourceIndicator> tileToResourceIndicator = new();

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    }

    private void UpdateIndicators(
        IEnumerable<Vector2I> newIndicatedTiles,
        IEnumerable<Vector2I> toRemoveTiles
    )
    {
        if (newIndicatedTiles.Any())
            audioStreamPlayer.Play();
        foreach (var newTile in newIndicatedTiles)
        {
            var indicator = resourceIndicatorScene.Instantiate<ResourceIndicator>();
            AddChild(indicator);
            indicator.GlobalPosition = newTile * 64;
            tileToResourceIndicator[newTile] = indicator;
        }
        foreach (var removeTile in toRemoveTiles)
        {
            tileToResourceIndicator.TryGetValue(removeTile, out var indicator);
            if (IsInstanceValid(indicator))
            {
                indicator.Destroy();
            }
            tileToResourceIndicator.Remove(removeTile);
        }
    }

    private void HandleResourceTileUpdated()
    {
        var currentResourceTiles = gridManager.GetCollectedResourceTiles();
        var newlyIndicatedTiles = currentResourceTiles.Except(indicatedTiles);
        var toRemoveTiles = indicatedTiles.Except(currentResourceTiles);

        indicatedTiles = currentResourceTiles;
        UpdateIndicators(newlyIndicatedTiles, toRemoveTiles);
    }

    private void OnResourceTilesUpdated(int _)
    {
        Callable.From(HandleResourceTileUpdated).CallDeferred();
    }
}
