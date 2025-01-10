using Godot;
using System;

public partial class PhysicalDialog : RigidBody2D
{
    private bool dragging = false;
    private Vector2 dragOffset = Vector2.Zero;

    public override void _Ready()
    {
        GetNode<PanelContainer>("PanelContainer").Connect(
            Control.SignalName.GuiInput,
            new Callable(this, MethodName.OnInputEvent)
        );
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && !mouse.Pressed)
        {
            dragging = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (dragging)
        {
            KinematicCollision2D collision = MoveAndCollide(GetGlobalMousePosition()-GlobalPosition+dragOffset);
            GodotObject collider = collision?.GetCollider();
            if (collider is PhysicalDialog body && !body.dragging)
            {
                body.ApplyForce(-collision.GetNormal()*1000f);
            }
        }
    }

    public void OnInputEvent(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
        {
            dragging = true;
            dragOffset = GlobalPosition - mouse.GlobalPosition;
        }
    }
}
