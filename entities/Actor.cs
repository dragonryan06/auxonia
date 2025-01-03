using System;
using System.Linq;
using Godot;

public partial class Actor : CharacterBody3D
{
    public enum NavState
    {
        IDLE,
        NAVIGATING, // Navigating, but not currently moving or turning
        MOVING,
        TURNING_LEFT,
        TURNING_RIGHT
    }
    public NavState navstate = NavState.IDLE;

    [Export]
    private float MOVE_TIME = 1.0f;
    [Export]
    private NodePath Map = "../Map";
    [Export]
    private Vector2I Destination = Vector2I.Zero;

    public Vector2I GridPosition = Vector2I.Zero;
    private Vector2I lastGridPosition = Vector2I.Zero;

    private Vector3[] path;
    private Vector2I[] idPath;

    public Timer walkCooldown;

    public override void _Ready()
    {
        DebugDraw3D.ScopedConfig().SetThickness(0.5f);

        walkCooldown = new Timer();
        walkCooldown.Timeout += () => TakeStep();
        AddChild(walkCooldown);
        walkCooldown.Start(MOVE_TIME);

        GridPosition = GetNode<GenericMap>(Map).WorldToGrid(Position);

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
            path = map.PointPath(GridPosition, Destination);
            idPath = map.IdPath(GridPosition, Destination);
            if (path.Length <= 1)
            {
                walkCooldown.Stop();
                navstate = NavState.IDLE;
            }
            else
            {
                walkCooldown.Start();
                navstate = NavState.NAVIGATING;
            }
        }
    }

    private void TakeStep()
    {
        GenericMap map = GetNode<GenericMap>(Map);

        Action navCallback = () => navstate = NavState.NAVIGATING; map.SetPointSolid(lastGridPosition, false);

        // Check if we can/should path to the next point
        if (path?.Length > 1 && (!map.IsPointSolid(idPath[1]) || idPath[1] == GridPosition))
        {
            float angleTo = Mathf.RadToDeg(((Vector2)GridPosition).AngleToPoint(new Vector2(idPath[1].X, idPath[1].Y)) + Rotation.Y);
            // Adding +90 because AngleToPoint is relative to X axis, and our character faces Z
            angleTo = Mathf.Wrap(angleTo + 90, -180, 180);

            if (angleTo == 0)
            {
                // We are facing the point; move forwards.
                Vector3 newPosition = Position;
                newPosition.X = path[1].X;
                newPosition.Z = path[1].Z;

                // If we moved diagonally, we take sqrt(a^2+b^2) time instead of just a.
                if (idPath[1].X != GridPosition.X && idPath[1].Y != GridPosition.Y)
                {
                    walkCooldown.WaitTime = Math.Sqrt(Math.Pow(MOVE_TIME, 2) + Math.Pow(MOVE_TIME, 2));
                }
                else
                {
                    walkCooldown.WaitTime = MOVE_TIME;
                }
                lastGridPosition = GridPosition;
                GridPosition = idPath[1];
                map.SetPointSolid(GridPosition, true);

                navstate = NavState.MOVING;
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(
                    this,
                    "position",
                    newPosition,
                    walkCooldown.WaitTime
                ).SetTrans(
                    Tween.TransitionType.Sine
                ).SetEase(
                    Tween.EaseType.InOut
                );
                tween.TweenCallback(Callable.From(navCallback));

                path = path.Skip(1).ToArray();
                idPath = idPath.Skip(1).ToArray();
            }
            else if (angleTo < 0)
            {
                // Turn right
                walkCooldown.WaitTime = MOVE_TIME;
                navstate = NavState.TURNING_RIGHT;
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(
                    this,
                    "rotation_degrees",
                    new Vector3(RotationDegrees.X,RotationDegrees.Y+45,RotationDegrees.Z),
                    walkCooldown.WaitTime
                ).SetTrans(
                    Tween.TransitionType.Sine
                ).SetEase(Tween.EaseType.InOut);
                tween.TweenCallback(Callable.From(navCallback));
            }
            else
            {
                // Turn left
                walkCooldown.WaitTime = MOVE_TIME;
                navstate = NavState.TURNING_LEFT;
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(
                    this,
                    "rotation_degrees",
                    new Vector3(RotationDegrees.X,RotationDegrees.Y-45,RotationDegrees.Z),
                    walkCooldown.WaitTime
                ).SetTrans(
                    Tween.TransitionType.Sine
                ).SetEase(Tween.EaseType.InOut);
                tween.TweenCallback(Callable.From(navCallback));
            }

            if (Game.DebugOverlay && path?.Length > 1)
            {
                DebugDraw3D.DrawPointPath(path, duration: (float)walkCooldown.WaitTime,points_color:Colors.DarkCyan,lines_color:Colors.Cyan);
                DebugDraw3D.DrawLine(map.GridToWorld(GridPosition), map.GridToWorld(Destination), Colors.Green,(float)walkCooldown.WaitTime);
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
