using Godot;

namespace Game;

public partial class GameCamera : Camera2D
{
    private readonly StringName ACTION_PAN_LEFT = "pan_left";
    private readonly StringName ACTION_PAN_RIGHT = "pan_right";
    private readonly StringName ACTION_PAN_UP = "pan_up";
    private readonly StringName ACTION_PAN_DOWN = "pan_down";
    private readonly int PAN_SPEED = 500;
    private readonly int TILE_SIZE = 64;

    public override void _Process(double delta)
    {
        GlobalPosition = GetScreenCenterPosition();
        var moveDir = Input.GetVector(
            ACTION_PAN_LEFT,
            ACTION_PAN_RIGHT,
            ACTION_PAN_UP,
            ACTION_PAN_DOWN
        );
        GlobalPosition += moveDir * PAN_SPEED * (float)delta;
    }

    public void SetBoundingRect(Rect2I boundingRect)
    {
        LimitLeft = boundingRect.Position.X * TILE_SIZE;
        LimitRight = boundingRect.End.X * TILE_SIZE;
        LimitTop = boundingRect.Position.Y * TILE_SIZE;
        LimitBottom = boundingRect.End.Y * TILE_SIZE;
    }

    public void CenterOnPosition(Vector2 position)
    {
        GlobalPosition = position;
    }
}
