using Godot;
using System;

public interface IBoardObject
{
    string EntityName { get; set; }
    Vector2I GridPosition { get; set; }
}

public partial class ItemPile : MeshInstance3D, IBoardObject, IItem
{
    public string EntityName { get; set; } = "ItemPile";
    public Vector2I GridPosition { get; set; }

    public ItemType Type { get; set; }
    public int Quantity { get; set; }

    public void FromItem(IItem item)
    {
        EntityName = item.EntityName;
        Type = item.Type;
        Quantity = item.Quantity;
    }

    public override void _Ready()
    {
        Mesh = (Mesh)Game.ItemPileMeshes.GetItemMesh(3).Duplicate();
    }
}