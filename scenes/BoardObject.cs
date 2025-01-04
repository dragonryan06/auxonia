using Godot;
using System;

public interface IBoardObject
{
    string Name { get; set; }
    Vector2I Position { get; set; }
}

public class ItemPile : IBoardObject, IItem
{
    public string Name { get; set; } = "ItemPile";
    public Vector2I Position { get; set; }

    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}