using Godot;
using System;
using System.Runtime.ConstrainedExecution;

public partial class AuxonInfo : PhysicalDialog
{

    public Auxon Target = null;
    private int lastTab = 0;

    public override void _Ready()
    {
        base._Ready();

        GetNode<TabBar>("TabBar").Connect(
            TabBar.SignalName.TabChanged,
            new Callable(this, MethodName.OnTabChanged)
        );
        Target.Connect(
            Auxon.SignalName.TaskBegun,
            new Callable(this, MethodName.UpdateView)
        );

        PanelContainer panel = GetNode<PanelContainer>("PanelContainer");
        Action readjustTabs = () => { GetNode<TabBar>("TabBar").Position = new Vector2(panel.Size.X + 48, panel.Size.Y - 406); };
        readjustTabs();
        panel.ItemRectChanged += readjustTabs;

        UpdateView();
    }

    public override void _Process(double delta)
    {
        GetNode<Camera3D>("PanelContainer/Metadata/Header/PanelContainer/Picture/SubViewport/Camera3D").LookAtFromPosition(Target.GetNode<Marker3D>("ChaseCameraPos").GlobalPosition, Target.GlobalPosition);
    }

    private void UpdateView()
    {
        GD.Print("UPDATE");
        if (IsInstanceValid(Target))
        {
            HBoxContainer header = GetNode<HBoxContainer>("PanelContainer/Metadata/Header");
            header.GetNode<Label>("VBoxContainer/Title").Text = Target.Name;
            header.GetNode<Label>("VBoxContainer/MarginContainer/Subtitle").Text = "Generic Automaton";
            header.GetNode<Label>("VBoxContainer/CurrentTask").Text = Target.ActiveTask?.Name ?? "Idle";
            header.GetNode<Label>("VBoxContainer/AdditionalNote").Text = "Holding: " + (Target.HeldItem?.EntityName ?? "Nothing");
            if (Target.HeldItem?.Quantity > 1) header.GetNode<Label>("VBoxContainer/AdditionalNote").Text += " [x" + Target.HeldItem.Quantity + "]";
        }
    }

    public void OnTabChanged(int tab)
    {
        if (lastTab != tab)
        {
            PanelContainer panel = GetNode<PanelContainer>("PanelContainer");
            panel.GetChild<Control>(lastTab).Hide();
            panel.GetChild<Control>(tab).Show();
            lastTab = tab;
        }
    }

    //public void 
}
