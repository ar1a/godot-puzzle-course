using System.Linq;
using Godot;

namespace Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
    [Signal]
    public delegate void DestroyAnimationFinishedEventHandler();

    [Export]
    private Texture2D maskTexture;

    [Export]
    private PackedScene impactParticlesScene;

    [Export]
    private PackedScene destroyParticlesScene;

    private Tween activeTween;
    private Node2D animationRootNode;
    private Sprite2D maskNode;

    public override void _Ready()
    {
        YSortEnabled = false;
        SetupNodes();
    }

    public void PlayInAnimation()
    {
        if (animationRootNode == null)
            return;
        if (activeTween?.IsValid() == true)
            activeTween.Kill();

        activeTween = CreateTween();
        activeTween
            .TweenProperty(animationRootNode, "position", Vector2.Zero, .3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In)
            .From(Vector2.Up * 128);
        activeTween.TweenCallback(
            Callable.From(() =>
            {
                var impactParticles = impactParticlesScene.Instantiate<Node2D>();
                Owner.GetParent().AddChild(impactParticles);
                impactParticles.GlobalPosition = GlobalPosition;
                GameCamera.Shake();
            })
        );
        activeTween
            .TweenProperty(animationRootNode, "position", Vector2.Up * 16, .1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        activeTween
            .TweenProperty(animationRootNode, "position", Vector2.Zero, .1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    public void PlayDestroyAnimation()
    {
        if (animationRootNode == null)
            return;
        if (activeTween?.IsValid() == true)
            activeTween.Kill();

        // snap to zero if destroyed during placing animation
        animationRootNode.Position = Vector2.Zero;

        maskNode.ClipChildren = ClipChildrenMode.Only;
        maskNode.Texture = maskTexture;

        var destroyParticles = destroyParticlesScene.Instantiate<Node2D>();
        Owner.GetParent().AddChild(destroyParticles);
        destroyParticles.GlobalPosition = GlobalPosition;
        GameCamera.Shake();

        activeTween = CreateTween();
        activeTween.TweenProperty(animationRootNode, "rotation_degrees", -5, .1);
        activeTween.TweenProperty(animationRootNode, "rotation_degrees", 5, .1);
        activeTween.TweenProperty(animationRootNode, "rotation_degrees", -2, .1);
        activeTween.TweenProperty(animationRootNode, "rotation_degrees", 2, .1);
        activeTween.TweenProperty(animationRootNode, "rotation_degrees", 0, .1);

        activeTween
            .TweenProperty(animationRootNode, "position", Vector2.Down * 300, .4)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.In);

        activeTween.Finished += () => EmitSignal(SignalName.DestroyAnimationFinished);
    }

    private void SetupNodes()
    {
        if (GetChildren().FirstOrDefault() is not Node2D spriteNode)
            return;
        RemoveChild(spriteNode);
        Position = spriteNode.Position;

        maskNode = new Sprite2D
        {
            Centered = false,
            Offset = new Vector2(-maskTexture.GetSize().X / 2, -maskTexture.GetSize().Y),
        };
        AddChild(maskNode);

        animationRootNode = new Node2D();
        maskNode.AddChild(animationRootNode);

        animationRootNode.AddChild(spriteNode);
        spriteNode.Position = Vector2.Zero;
    }
}
