using Godot;
using System;

public partial class Game : Node
{
    public static Actor Selected { set; get; }

    public static bool DebugOverlay { set; get; }

    public static MeshLibrary ItemPileMeshes;

    public override void _Ready()
    {
        ItemPileMeshes = GD.Load<MeshLibrary>("res://resources/models/testmeshlib.tres");

        DebugOverlay = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("toggle_debug_overlay")) { DebugOverlay = !DebugOverlay; }
    }
}
