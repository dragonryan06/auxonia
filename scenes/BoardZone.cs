using System;
using Godot;
public interface IBoardZone
{
    string EntityName { get; set; }
    Rect2I Bounds { get; set; }
}

public partial class MiningZone : Node, IBoardZone
{
    public string EntityName { get; set; }
    public Rect2I Bounds { get; set; }
}