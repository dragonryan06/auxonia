using Godot;
using System;
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
}

public partial class Auxon : Actor
{
    [Signal]
    public delegate void TaskCompletedEventHandler();

    public Task[] TaskQueue;
    public Task? ActiveTask;

    public override void _Ready()
    {
        base._Ready();

        PathFinished += () => EmitSignal(SignalName.TaskCompleted);
        TaskCompleted += () => AssignTask(PopNextTaskOrNull());
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
                    } // FOR OTHER CASES start a like tick timer that will up the work amount or something...
            }
        }
        else
        {
            GD.Print("Auxon finished every task!!! probably handle this with a statemachine somehow....");
        }
    }

    public Task? PopNextTaskOrNull()
    {
        if (TaskQueue.Length > 0)
        {
            Task next = TaskQueue[0];
            TaskQueue = TaskQueue.Skip(1).ToArray();
            return next;
        }
        else return null;
    }
}
