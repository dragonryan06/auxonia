using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Actor : CharacterBody3D
{
    [Export]
    float MOVE_TIME = 1.0f;

    [Export]
    NodePath Map = "../Map";

    [Export]
    Vector2I destination = Vector2I.Zero;

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
    }

    private void TakeStep()
    {
        Vector3 newPosition = Position;
        GenericMap map = GetNode<GenericMap>(Map);

        if (!path.IsEmpty())
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
            gridPosition = idPath[0];

            if (path.Length > 1) DebugDraw3D.DrawPointPath(path,duration:(float)walkCooldown.WaitTime);

            path = path.Skip(1).ToArray();
            idPath = idPath.Skip(1).ToArray();
        }
        else
        {
            path = map.PointPath(gridPosition, destination);
            idPath = map.IdPath(gridPosition, destination);
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
}
