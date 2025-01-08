using System;
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

    public bool TryMineChunk(Vector2I position)
    {
        if (GD.Randi() < mineSuccessChance)
        {
            OreChunk chunk = new OreChunk();
            chunk.EntityName = "Iron Ore Chunk";
            chunk.Type = ItemType.OreIron;
            chunk.Quantity = 1;
            Board.PlaceItem(position, chunk);
            return true;
        }
        else return false;
    }
}