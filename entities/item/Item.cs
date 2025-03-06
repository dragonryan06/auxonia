using Godot;
using System;

public enum ItemType
{
    Nothing,
    OreIron,
    IngotSteel,
    ToolPickaxe
}

public interface IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}


public interface IEquipable : IItem
{
   
}

public class OreChunk : IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}

public class Tool : IEquipable
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; } = 1;
}

public class TestItemObject : IItem
{
    public string EntityName { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
}