using System;
using System.Linq;
using Godot;

// A pathfinding entity.
public partial class Actor : CharacterBody3D
{
    [Signal]
    public delegate void PathFinishedEventHandler();

    public enum NavState
    {
        Idle,
        Navigating, // Navigating, but not currently moving or turning
        Moving,
        TurningLeft,
        TurningRight
    }
    public NavState navstate = NavState.Idle;

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

    private GenericMap map = null;

    public Timer walkCooldown;

    public override void _Ready()
    {
        DebugDraw3D.ScopedConfig().SetThickness(0.5f);

        walkCooldown = new Timer();
        walkCooldown.Timeout += () => TakeStep();
        AddChild(walkCooldown);
        walkCooldown.Start(MOVE_TIME);

        map = GetNode<GenericMap>(Map);
        GridPosition = map.WorldToGrid(Position);
    }

    public void MoveTo(Vector2I positionID)
    {
        Destination = positionID;
        UpdatePath();
    }

    private void UpdatePath()
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
                EmitSignal(SignalName.PathFinished);
                navstate = NavState.Idle;
            }
            else
            {
                walkCooldown.Start();
                navstate = NavState.Navigating;
            }
        }
    }

    private void TakeStep()
    {
        Action navCallback = () => navstate = NavState.Navigating; map.SetPointSolid(lastGridPosition, false);

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

                navstate = NavState.Moving;
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
                navstate = NavState.TurningRight;
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
                navstate = NavState.TurningLeft;
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

            if (Game.NavDebugOverlay && path?.Length > 1)
            {
                DebugDraw3D.DrawPointPath(path, duration: (float)walkCooldown.WaitTime,points_color:Colors.DarkCyan,lines_color:Colors.Cyan);
                DebugDraw3D.DrawLine(map.GridToWorld(GridPosition), map.GridToWorld(Destination), Colors.Green,(float)walkCooldown.WaitTime);
            }
            walkCooldown.Start();
        }
        // Try to find a new path
        else
        {
            UpdatePath();
        }
    }
}
