using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Godot;
public interface IBoardZone
{
    public GameBoard Board { get; set; }

    string EntityName { get; set; }
    Rect2I Bounds { get; set; }
}

public partial class MiningZone : Node, IBoardZone
{
    public GameBoard Board { get; set; }
    public string EntityName { get; set; }
    public Rect2I Bounds { get; set; }
    public float mineSuccessChance = 0.5f;

    // 8 neighboring cells
    private static readonly Vector2I[] chunkSpawns =
    {
        new Vector2I(-1,1),
        new Vector2I(0,1),
        new Vector2I(1,1),
        new Vector2I(-1,0),
        new Vector2I(1,0),
        new Vector2I(-1,-1),
        new Vector2I(0,-1),
        new Vector2I(1,-1)
    };

    public bool TryMineChunk(Vector2I position)
    {
        if (GD.Randf() < mineSuccessChance)
        {
            List<Vector2I> valid = new();
            foreach (Vector2I spawn in chunkSpawns)
            {
                if (!Board.IsCellOccupied(spawn+position)) valid.Add(position+spawn);
            }

            if (valid.Count > 0)
            {
                Vector2I placement = valid[Math.Abs((int)GD.Randi()) % valid.Count];
                OreChunk chunk = new OreChunk();
                chunk.EntityName = "Iron Ore Chunk";
                chunk.Type = ItemType.OreIron;
                chunk.Quantity = 1;
                Board.PlaceItem(placement, chunk);
                return true;
            }
            else return false;
        }
        else return false;
    }
}