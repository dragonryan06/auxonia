using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TaskType
{
    Move,
    Mine,
    Haul
}

public struct Task : IComparable<Task>
{
    public Task(Vector2I position, TaskType type, int priority)
    {
        Position = position;
        Type = type;
        Priority = priority;
    }

    public Task(Vector2I position, IBoardZone zone, TaskType type, int priority)
    {
        Position = position;
        Zone = zone;
        Type = type;
        Priority = priority;
    }

    public string Name = "Unnamed Task";
    public TaskType Type;
    public Vector2I Position;
    public IBoardZone? Zone = null;
    public Auxon? Owner = null;

    public int Priority = 0;

    public int Work = 0;
    public int WorkToFinish = -1;

    public readonly override string ToString()
    {
        return Type.ToString() + Position;
    }

    public readonly int CompareTo(Task other)
    {
        return Priority - other.Priority;
    }
}

public partial class Auxon : Actor, ISelectable
{
    [Signal]
    public delegate void TaskEnqueuedEventHandler();
    [Signal]
    public delegate void TaskBegunEventHandler();
    [Signal]
    public delegate void TaskCompletedEventHandler();

    private PackedScene infoScene;
    private PackedScene doAfter;

    private AuxonInfo? infoDialog;

    // ISelectable
    private bool selected = false;
    public bool Selected 
    {  
        get 
        {
            return selected;
        }
        set
        {
            ShaderMaterial mat = (ShaderMaterial)GetNode<MeshInstance3D>("Skeleton3D/LeftTrack/LeftTrack").GetActiveMaterial(0).NextPass;
            if (value)
            {
                mat.SetShaderParameter("outline_color", Colors.DarkOrange);
            } 
            else
            {
                mat.SetShaderParameter("outline_color", Colors.Black);
            }
            selected = value;
        } 
    }
    private bool hovered = false;
    public bool Hovered
    {
        get
        {
            return hovered;
        }
        set
        {
            ShaderMaterial mat = (ShaderMaterial)GetNode<MeshInstance3D>("Skeleton3D/LeftTrack/LeftTrack").GetActiveMaterial(0).NextPass;
            if (value && !Selected)
            {
                mat.SetShaderParameter("outline_color", Colors.Yellow);
            }
            else if (!Selected)
            {
                mat.SetShaderParameter("outline_color", Colors.Black);
            }
            hovered = value;
        }
    }

    // Task
    public List<Task> TaskQueue;
    public Task? ActiveTask;

    // Inventory
    public IItem? HeldItem = null;

    public override void _Ready()
    {
        base._Ready();
        infoScene = GD.Load<PackedScene>("res://ui/dialogs/auxon_info.tscn");
        doAfter = GD.Load<PackedScene>("res://ui/3d/do_after_bar.tscn");

        TaskQueue = new List<Task>();
        ActiveTask = null;

        TaskEnqueued += () => { if (ActiveTask is null) AssignTask(PopNextTaskOrNull()); };
        PathFinished += () => EmitSignal(SignalName.TaskCompleted);
        TaskCompleted += () => AssignTask(PopNextTaskOrNull());

        Connect(
            CollisionObject3D.SignalName.InputEvent,
            new Callable(this, MethodName.OnInputEvent)
        );
        Connect(
            CollisionObject3D.SignalName.MouseEntered,
            new Callable(this, MethodName.OnMouseEntered)
        );
        Connect(
            CollisionObject3D.SignalName.MouseExited,
            new Callable(this, MethodName.OnMouseExited)
        );
    }

    public void EnqueueTask(Task task)
    {
        // Eventually have like priorities cause stuff to get pushed around
        task.Owner = this;
        TaskQueue.Add(task);
        EmitSignal(SignalName.TaskEnqueued);
    }

    public void AssignTask(Task? t)
    {
        if (t != null)
        {
            Task task = t.Value;
            ActiveTask = task;
            switch (task.Type)
            {
                case TaskType.Move:
                    {
                        MoveTo(task.Position);
                        break;
                    }
                case TaskType.Mine:
                    {
                        DoAfter bar = doAfter.Instantiate<DoAfter>();
                        bar.Position = Vector3.Up * 3.5f;
                        bar.MaxTicks = 100;
                        AddChild(bar);

                        Action callback = () =>
                        {
                            bar.QueueFree();

                            MiningZone zone = (MiningZone)task.Zone;
                            if (!zone.TryMineChunk(task.Position))
                            {
                                // Continue mining until success
                                Task keepMining = new Task(task.Position, task.Zone, task.Type, task.Priority);
                                keepMining.Name = task.Name;
                                EnqueueTask(keepMining);
                            }
                            EmitSignal(SignalName.TaskCompleted);
                        };

                        bar.Start(callback);
                        break;
                    }
            }
        }
        else
        {
            ActiveTask = null;
            GD.Print("Auxon finished every task!!! probably handle this with a statemachine somehow....");
        }
        EmitSignal(SignalName.TaskBegun);
    }

    public Task? PopNextTaskOrNull()
    {
        if (TaskQueue.Count > 0)
        {
            TaskQueue.Sort();
            Task next = TaskQueue.First();
            TaskQueue.RemoveAt(0);
            return next;
        }
        else return null;
    }

    public void OnInputEvent(Camera3D camera, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNorm, int shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
        {
            Selected = !Selected;
            Hovered = true;
            Game.Selected = this;

            World w = (World)FindParent("World");
            if (!IsInstanceValid(infoDialog))
            {
                infoDialog = infoScene.Instantiate<AuxonInfo>();
                infoDialog.Target = this;
                w.AddDialog(infoDialog);
            } else
            {
                w.RemoveDialog(infoDialog);
                infoDialog.QueueFree();
            }
        }
    }

    public void OnMouseEntered()
    {
        Hovered = true;
    }

    public void OnMouseExited()
    {
        Hovered = false;
    }
}
