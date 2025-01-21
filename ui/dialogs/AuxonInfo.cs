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
        if (IsInstanceValid(Target))
        {
            GetNode<Label>("PanelContainer/Metadata/Header/VBoxContainer/Title").Text = Target.Name;
            GetNode<Label>("PanelContainer/Metadata/Header/VBoxContainer/MarginContainer/Subtitle").Text = "Generic Automaton";
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
