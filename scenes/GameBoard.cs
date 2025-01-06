using Godot;
using System;
using System.Linq;

public partial class GameBoard : Node
{
    private enum BoardMeshes
    {
        Ingots,
        Ingots2,
        Ingots3,
        Ingots4
    }

	private GenericMap map;

    // As opposed to the godot collection of the same name...
    private System.Collections.Generic.Dictionary<Vector2I, IBoardObject> boardData;
	private System.Collections.Generic.Dictionary<Rect2I, IBoardZone> boardZones;

	public bool PlaceItem(Vector2I pos, IItem item)
	{
		if (boardData.TryGetValue(pos, out IBoardObject existing))
		{
			if (existing is ItemPile)
			{
				// Stack item into this pile
				GD.Print("Stacked item into existing pile");
				return true;
			}
			else return false;
		}
		else
		{
			ItemPile newPile = new ItemPile();
			newPile.GridPosition = pos;
			newPile.Position = map.GridToWorld(pos);
			newPile.FromItem(item);
			map.SetPointSolid(pos, true);
			boardData.Add(pos, newPile);
			AddChild(newPile);
			return true;
		}
		
		
	}

	public Vector2I[] GetUsedCells()
	{
		return boardData.Keys.ToArray();
	}

	public IBoardObject PopAt(Vector2I pos)
	{
		IBoardObject obj = boardData[pos];
		boardData.Remove(pos);
		return obj;
	}

	public IBoardObject GetObject(Vector2I pos)
	{
		return boardData[pos];
	}
	
	public override void _Ready()
	{
		map = GetParent<GenericMap>();
		boardData = new System.Collections.Generic.Dictionary<Vector2I, IBoardObject>();
		boardZones = new System.Collections.Generic.Dictionary<Rect2I, IBoardZone>();

    }

    public override void _Process(double delta)
    {
        if (Game.DebugOverlay)
        {
            foreach (Rect2I rect in boardZones.Keys)
			{
				DebugDraw3D.DrawAabb(new Aabb(map.GridToWorld(rect.Position), map.GridToWorld(rect.Size)), Colors.Yellow);
			}
        }
    }
}
