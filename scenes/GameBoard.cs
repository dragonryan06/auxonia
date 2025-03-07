using Godot;
using System;
using System.Collections.Generic;
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

	public bool IsCellOccupied(Vector2I pos)
	{
		return boardData.TryGetValue(pos, out _) || map.IsPointSolid(pos);
	}

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

	public List<IBoardZone> ZoneLookup(Vector2I pos)
	{
		List<IBoardZone> hits = new List<IBoardZone>();
		foreach (IBoardZone zone in boardZones.Values)
		{
			if (zone.Bounds.HasPoint(pos)) hits.Add(zone);
		}
		return hits;
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

		MiningZone testMineZone = new MiningZone();
		testMineZone.Bounds = new Rect2I(80, 64, 16, 16);
		testMineZone.Board = this;
		boardZones.Add(testMineZone.Bounds, testMineZone);
    }

    public override void _Process(double delta)
    {
        if (Game.NavDebugOverlay)
        {
            foreach (Rect2I rect in boardZones.Keys)
			{
				DebugDraw3D.DrawAabb(
					new Aabb(map.GridToWorld(rect.Position) - new Vector3(map.CellSize.X/2.0f, 0, map.CellSize.Y/2.0f), 
					new Vector3(rect.Size.X*map.CellSize.X, 0.25f, rect.Size.Y*map.CellSize.Y)), 
					Colors.Yellow
				);
			}
        }
    }
}
