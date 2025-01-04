using Godot;
using System;

public enum ItemType
{
    Nothing,
    IngotSteel
}

public interface IItem
{
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}
