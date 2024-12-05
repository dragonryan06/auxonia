using Godot;
using System;

public partial class Actor : CharacterBody3D
{
    [Export]
    float MOVE_SPEED = 1.0f; 
    [Export]
    Vector2 destination = new Vector2();

    public override void _PhysicsProcess(double delta)
    {
        Vector3 newVelocity = Velocity;
        if (!IsOnFloor())
        {
            newVelocity.Y -= 9.81f*(float)delta;
        }

        if (Position.X != destination.X && Position.Z != destination.Y)
        {
            Vector2 pos2D = new Vector2(Position.X, Position.Z);
            newVelocity.X = pos2D.AngleToPoint(destination) * MOVE_SPEED;
            newVelocity.Z = pos2D.AngleToPoint(destination) * MOVE_SPEED;
        }

        Velocity = newVelocity;
        MoveAndSlide();
    }
}
