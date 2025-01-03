using Godot;
using System;

[Tool]
public partial class World : Node3D
{
    // NOTE: You must save the scene and then reload it to change levels from the editor.
	[Export(PropertyHint.File)]
	public string Level = "res://scenes/levels/test_plane.tscn";

    public void LoadLevel()
	{
        MeshInstance3D old = GetNode<MeshInstance3D>("Level");
        if (IsInstanceValid(old))
        {
            RemoveChild(old);
            old.QueueFree();
        }

        MeshInstance3D loaded = (MeshInstance3D)GD.Load<PackedScene>(Level).Instantiate();
        AddChild(loaded);
        loaded.Owner = GetTree().EditedSceneRoot;
    }

	public override void _Ready()
	{
		LoadLevel();
	}
}
