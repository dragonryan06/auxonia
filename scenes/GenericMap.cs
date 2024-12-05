using Godot;
using System;

public partial class GenericMap : CsgMesh3D
{
    public override void _Ready()
    {
        DebugDraw3D.ScopedConfig().SetThickness(0.25f);
    }

    public override void _Process(double delta)
    {
        DebugDraw3D.DrawBox(Vector3.Zero, Quaternion.Identity, new Vector3(100, 100, 100), Colors.Red,true,100000);
    }
}
