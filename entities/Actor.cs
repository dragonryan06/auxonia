using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Actor : CharacterBody3D
{
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
        Vector3 newPosition = Position;
        GenericMap map = GetNode<GenericMap>(Map);

        if (!path.IsEmpty() && (!map.IsPointSolid(idPath[0]) || idPath[0] == gridPosition))
        {
            newPosition.X = path[0].X;
            newPosition.Z = path[0].Z;

            // If we moved diagonally, we take sqrt(a^2+b^2) time instead of just a.
            if (idPath[0].X != gridPosition.X && idPath[0].Y != gridPosition.Y)
            {
                walkCooldown.WaitTime = Math.Sqrt(Math.Pow(MOVE_TIME,2)+Math.Pow(MOVE_TIME,2));
            }
            else
            {
                walkCooldown.WaitTime = MOVE_TIME;
            }
            map.SetPointSolid(gridPosition, false);
            gridPosition = idPath[0];
            map.SetPointSolid(gridPosition, true);

            if (path.Length > 1) DebugDraw3D.DrawPointPath(path,duration:(float)walkCooldown.WaitTime);

            path = path.Skip(1).ToArray();
            idPath = idPath.Skip(1).ToArray();
        }
        else
        {
            UpdatePath(map);
        }

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(
            this, 
            "position", 
            newPosition, 
            walkCooldown.WaitTime*0.75
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        walkCooldown.Start();
    }

    private void OnInputEvent(Camera3D camera, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNorm, int shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
        {
            Game.selected = this;
        }
    }
}
