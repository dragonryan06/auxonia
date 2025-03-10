using Godot;
using System;

public partial class DoAfter : MeshInstance3D
{
    int _tickCount = 0;
    public int TickCount
    {
        get { return _tickCount; }
        set
        {
            _tickCount = value;
            Update();
        }
    }

    int _maxTicks = 0;
    public int MaxTicks
    {
        get { return _maxTicks; }
        set
        {
            _maxTicks = value;
            Update();
        }
    }

    public void Start(Action callback)
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "TickCount", 100, 2.0);
        tween.TweenCallback(Callable.From(callback));
    }

    private void Update()
    {
        ShaderMaterial mat = (ShaderMaterial)GetActiveMaterial(0);
        mat.SetShaderParameter("percentage", (float)_tickCount / (float)_maxTicks);
    }
}
