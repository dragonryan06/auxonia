using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GenericMap : CsgMesh3D
{
    [Export]
    private Vector2I MapDimensions = new Vector2I(128,128);

    [Export]
    private Vector2I CellSize = new Vector2I(4,4);

    private AStarGrid2D navGrid = new AStarGrid2D();

    public override void _Ready()
    {
        navGrid.Region = new Rect2I(Vector2I.Zero, MapDimensions);
        navGrid.CellSize = CellSize;
        navGrid.Update();

        GetChild<CollisionObject3D>(0).Connect(
            CollisionObject3D.SignalName.InputEvent, 
            new Callable(this, MethodName.OnInputEvent)
        );
    }

    public Vector3[] PointPath(Vector2I fromID, Vector2I toID)
    {
        List<Vector3> path3D = new List<Vector3>();
        foreach (Vector2 p in navGrid.GetPointPath(fromID, toID))
        {
            // ID (0,0) is the top left corner of the map
            path3D.Add(new Vector3((p.X+2.0f)-MapDimensions.X*0.5f*CellSize.X, 0f, (p.Y+2.0f)-MapDimensions.Y*0.5f*CellSize.Y));
        }

        return path3D.ToArray();
    }

    public Vector2I[] IdPath(Vector2I fromID, Vector2I toID)
    {
        return navGrid.GetIdPath(fromID, toID).ToArray();
    }

    public Vector2I WorldToGrid(Vector3 pos)
    {
        return new Vector2I(
            (int)Math.Round(((pos.X - 2.0f) / CellSize.X) + (MapDimensions.X * 0.5f), 0),
            (int)Math.Round(((pos.Z - 2.0f) / CellSize.Y) + (MapDimensions.Y * 0.5f), 0)
        );
    }

    private void OnInputEvent(Camera3D camera, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNorm, int shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
        {
            DebugDraw3D.DrawRay(eventPos, eventNorm,4,duration:10);
            if (Game.selected != null) Game.selected.MoveTo(this, WorldToGrid(eventPos));
        }
    }
}
