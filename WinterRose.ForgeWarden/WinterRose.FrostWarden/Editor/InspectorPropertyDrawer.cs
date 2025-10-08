using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Editor;

/// <summary>
/// Use <see cref="InspectorPropertyDrawer{TFor, TContent}"/> instead!
/// </summary>
public abstract class InspectorPropertyDrawer
{
    internal UIContent Content { get; }
    internal protected MemberData MemberData { get; internal set; }
    internal protected object Target { get; internal set; }
    protected TrackedValue TrackedValue { get; private set; }

    public InspectorPropertyDrawer(UIContent content)
    {
        Content = content;
    }

    internal void InitInternal()
    {
        TrackedValue = new TrackedValue(Target, MemberData.Name);
        Init();
    }

    internal protected abstract void Init();
    internal void TickInternal()
    {
        Tick();
        if (TrackedValue.HasValueChanged())
            ValueUpdated();
    }
    internal protected virtual void Tick() { }
    internal protected abstract void ValueUpdated();
    internal void Draw(Rectangle bounds)
    {
        Content.InternalDraw(bounds);
    }
}

internal class NoCustomDrawer() : InspectorPropertyDrawer(new UIText(""))
{
    protected internal override void Init() =>
        ((UIText)Content).Text = MemberData.Name + " = " + MemberData.GetValue(Target)?.ToString() ?? "null";
    protected internal override void ValueUpdated() =>
        ((UIText)Content).Text = MemberData.Name + " = " + MemberData.GetValue(Target)?.ToString() ?? "null";
}

/// <summary>
/// Sub classes should have a constructor taking no arguments
/// </summary>
public abstract class InspectorPropertyDrawer<TFor, TContent>(TContent content) : InspectorPropertyDrawer(content) where TContent : UIContent
{
    protected new TContent Content => (TContent)base.Content;
}

public class BooleanPropertyDrawer() : InspectorPropertyDrawer<bool, UICheckBox>(new UICheckBox(""))
{
    protected internal override void Init()
    {
        Content.Text = MemberData.Name;
        Content.SetCheckedNoEvent((bool?)TrackedValue.Value ?? false);
        if (!MemberData.CanWrite)
            Content.ReadOnly = true;
        else
            Content.OnCheckedChangedBasic.Subscribe(OnChanged);
    }

    private void OnChanged(bool b)
    {
        TrackedValue.Set(b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetCheckedNoEvent((bool?)TrackedValue.Value ?? false);
    }
}

public class IntegerPropertyDrawer() : InspectorPropertyDrawer<int, UINumericUpDown<int>>(new UINumericUpDown<int>())
{
    protected internal override void Init()
    {
        Content.Label = MemberData.Name;
        Content.SetValue((int?)TrackedValue.Value ?? 0, false);
        if (!MemberData.CanWrite)
            Content.ReadOnly = true;
        else
            Content.OnValueChangedBasic.Subscribe(OnChanged);
    }

    private void OnChanged(int b)
    {
        TrackedValue.Set(b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetValue((int?)TrackedValue.Value ?? 0, false);
    }
}