using Godot;
using System;

public partial class Game : Node
{
    public static ISelectable? Selected = null;

    public static bool NavDebugOverlay { set; get; }
    public static bool TaskDebugOverlay { set; get; }

    public static MeshLibrary ItemPileMeshes;

    public override void _Ready()
    {
        ItemPileMeshes = GD.Load<MeshLibrary>("res://resources/models/testmeshlib.tres");

        NavDebugOverlay = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("toggle_debug_overlay")) { NavDebugOverlay = !NavDebugOverlay; }
    }
}
