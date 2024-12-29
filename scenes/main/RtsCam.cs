using Godot;
using System;

public partial class RtsCam : Camera3D
{
    // feeling kinda lazy right now but these values would actually be sensible if the velocity was instead multiplied by delta when applied
    [Export]
    private float MOVE_SPEED = 0.075f;
    [Export]
    private float ZOOM_SPEED = 0.05f;
    [Export]
    private float MAX_VELOCITY = 4.0f;
    [Export]
    private CurveXyzTexture ZoomCurve;

    private Vector2 MoveVelocity = new Vector2(0.0f, 0.0f);
    private float ZoomVelocity = 0.0f;

    float _zoomPosition = 0.0f;
    public float ZoomPosition
    {
        get { return _zoomPosition; }
        set
        {
            _zoomPosition = Mathf.Clamp(value, -1.0f, 1.0f);
            var newPosition = Position;
            var newRotation = Rotation;
            if (_zoomPosition >= 0.0f)
            {
                newPosition.Z = -ZoomCurve.CurveX.Sample(_zoomPosition);
                newPosition.Y = ZoomCurve.CurveY.Sample(_zoomPosition);
                newRotation.X = ZoomCurve.CurveZ.Sample(_zoomPosition);
            }
            else
            {
                newPosition.Z = -ZoomCurve.CurveX.Sample(-_zoomPosition);
                newPosition.Y = -ZoomCurve.CurveY.Sample(-_zoomPosition);
                newRotation.X = (3.0f * Mathf.Pi / 2.0f) - ZoomCurve.CurveZ.Sample(-_zoomPosition);
            }
            Position = newPosition;
            Rotation = newRotation;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent) 
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelUp when !mouseEvent.IsActionPressed("camera_zoom_in"):
                    ZoomVelocity += 3.0f * ZOOM_SPEED;
                    break;
                case MouseButton.WheelDown when !mouseEvent.IsActionPressed("camera_zoom_out"):
                    ZoomVelocity -= 3.0f * ZOOM_SPEED;
                    break;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = new Vector2(
                Input.GetAxis("camera_move_left", "camera_move_right"),
                Input.GetAxis("camera_move_forward", "camera_move_backward")
        ).Normalized();
        float zoomDirection = Input.GetAxis("camera_zoom_out", "camera_zoom_in");

        if (direction.X != 0.0f) MoveVelocity.X += direction.X * MOVE_SPEED;
        else MoveVelocity.X = (float)Mathf.MoveToward(MoveVelocity.X, 0.0, MOVE_SPEED);
        if (direction.Y != 0.0f) MoveVelocity.Y += direction.Y * MOVE_SPEED;
        else MoveVelocity.Y = (float)Mathf.MoveToward(MoveVelocity.Y, 0.0, MOVE_SPEED);
        if (zoomDirection != 0.0f) ZoomVelocity += zoomDirection * ZOOM_SPEED;
        else ZoomVelocity = (float)Mathf.MoveToward(ZoomVelocity, 0.0, ZOOM_SPEED);

        MoveVelocity = MoveVelocity.Clamp(-MAX_VELOCITY, MAX_VELOCITY);
        ZoomVelocity = Mathf.Clamp(ZoomVelocity, -MAX_VELOCITY, MAX_VELOCITY);

        Vector3 newPos = GetParent<Node3D>().Position;
        newPos.X += MoveVelocity.X;
        newPos.Z += MoveVelocity.Y;
        GetParent<Node3D>().Position = newPos;

        ZoomPosition += ZoomVelocity / 250.0f;
    }
}
