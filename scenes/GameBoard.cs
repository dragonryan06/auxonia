using Godot;
using System;

public partial class GameBoard : GridMap
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

	public void PlaceObject(Vector2I pos, IBoardObject obj)
	{
		boardData.Add(pos, obj);
		
		if (obj is ItemPile itemPile)
		{
			switch(itemPile.Type)
			{
				case ItemType.Nothing:
				{
					// Just thinking... eventually we will need many GridMaps under this class, especially if you want, for example, steel ingots and nothing ingots having different materials
					// I wonder if then like every item would need a separate GridMap??? Why do GridMaps suck so much...
					SetCellItem(new Vector3I(pos.X - map.MapDimensions.X / 2, 0, pos.Y - map.MapDimensions.Y / 2), (int)BoardMeshes.Ingots);
					break;
				}
			}
		}
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
	}
}
