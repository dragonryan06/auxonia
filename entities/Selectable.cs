using System;
using Godot;

public interface ISelectable
{
    public bool Selected { get;  set; }
    public bool Hovered { get; set; }

    public void OnInputEvent(Camera3D camera, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNorm, int shapeIdx);
    public void OnMouseEntered();
    public void OnMouseExited();
}
