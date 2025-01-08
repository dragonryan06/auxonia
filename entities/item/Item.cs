using Godot;
using System;

public enum ItemType
{
    Nothing,
    OreIron,
    IngotSteel
}

public interface IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}

public class OreChunk : IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}

public class TestItemObject : IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}