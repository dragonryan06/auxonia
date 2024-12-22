using System;
using System.Linq;
using Godot;

public partial class Actor : CharacterBody3D
{
    [Signal]
    public delegate void MoveEventHandler();
    [Signal]
    public delegate void RotateEventHandler(bool toLeft);

    [Export]
    private float MOVE_TIME = 1.0f;
    [Export]
    private NodePath Map = "../Map";
    [Export]
    private Vector2I Destination = Vector2I.Zero;

    private Vector2I gridPosition = Vector2I.Zero;

    private Vector3[] path;
    private Vector2I[] idPath;

    private Timer walkCooldown;

    public override void _Ready()
    {
        DebugDraw3D.ScopedConfig().SetThickness(1.0f);

        walkCooldown = new Timer();
        walkCooldown.Timeout += () => TakeStep();
        AddChild(walkCooldown);
        walkCooldown.Start(MOVE_TIME);

        gridPosition = GetNode<GenericMap>(Map).WorldToGrid(Position);

        Connect(
            CollisionObject3D.SignalName.InputEvent,
            new Callable(this, MethodName.OnInputEvent)
        );
    }

    public void MoveTo(GenericMap map, Vector2I positionID)
    {
        Destination = positionID;
        UpdatePath(map);
    }

    private void UpdatePath(GenericMap map)
    {
        if (map == null)
        {
            path = new Vector3[0];
            idPath = new Vector2I[0];
        }
        else
        {
            path = map.PointPath(gridPosition, Destination);
            idPath = map.IdPath(gridPosition, Destination);
        }
    }

    private void TakeStep()
    {
        GenericMap map = GetNode<GenericMap>(Map);

        // Check if we can/should path to the next point
        if (path?.Length > 1 && (!map.IsPointSolid(idPath[1]) || idPath[1] == gridPosition))
        {
            if (Game.DebugOverlay)
            {
                DebugDraw3D.DrawPointPath(path, duration: (float)walkCooldown.WaitTime);
                DebugDraw3D.DrawLine(map.GridToWorld(gridPosition), map.GridToWorld(Destination), duration: (float)walkCooldown.WaitTime);
            }

            float angleTo = Mathf.RadToDeg(((Vector2)gridPosition).AngleToPoint(new Vector2(idPath[1].X, idPath[1].Y)) + Rotation.Y);
            // Adding +90 because AngleToPoint is relative to X axis, and our character faces Z
            angleTo = Mathf.Wrap(angleTo + 90, -180, 180);

            if (angleTo == 0)
            {
                // We are facing the point; move forwards.
                Vector3 newPosition = Position;
                newPosition.X = path[1].X;
                newPosition.Z = path[1].Z;

                // If we moved diagonally, we take sqrt(a^2+b^2) time instead of just a.
                if (idPath[1].X != gridPosition.X && idPath[1].Y != gridPosition.Y)
                {
                    walkCooldown.WaitTime = Math.Sqrt(Math.Pow(MOVE_TIME, 2) + Math.Pow(MOVE_TIME, 2));
                }
                else
                {
                    walkCooldown.WaitTime = MOVE_TIME;
                }
                map.SetPointSolid(gridPosition, false);
                gridPosition = idPath[1];
                map.SetPointSolid(gridPosition, true);

                Position = newPosition;

                path = path.Skip(1).ToArray();
                idPath = idPath.Skip(1).ToArray();

                EmitSignal(SignalName.Move);
            }
            else if (angleTo < 0)
            {
                // The point is towards our right; turn.
                Vector3 newRotation = RotationDegrees;
                newRotation.Y += 45;
                RotationDegrees = newRotation;

                EmitSignal(SignalName.Rotate, false);
            }
            else
            {
                // The point is towards our left; turn.
                Vector3 newRotation = RotationDegrees;
                newRotation.Y -= 45;
                RotationDegrees = newRotation;

                EmitSignal(SignalName.Rotate, true);
            }

            walkCooldown.Start();
        }
        // Try to find a new path
        else
        {
            UpdatePath(map);
        }
    }

    private void OnInputEvent(Camera3D camera, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNorm, int shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
        {
            Game.Selected = this;
        }
    }
}
