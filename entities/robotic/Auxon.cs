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

public struct Task
{
    public Task(Vector2I position, TaskType type)
    {
        Position = position;
        Type = type;
    }

    public string Name = "Unnamed Task";
    public TaskType Type;
    public Vector2I Position;
    public Auxon Owner = null;

    public int Work = 0;
    public int WorkToFinish = -1;

    public override string ToString()
    {
        return Type.ToString() + Position;
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
                    } // FOR OTHER CASES start a like do-after that will tick up the work amount or something...
            }
            EmitSignal(SignalName.TaskBegun);
        }
        else
        {
            GD.Print("Auxon finished every task!!! probably handle this with a statemachine somehow....");
        }
    }

    public Task? PopNextTaskOrNull()
    {
        if (TaskQueue.Count > 0)
        {
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
