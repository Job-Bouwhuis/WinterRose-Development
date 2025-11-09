using Raylib_cs;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.Recordium;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Editor;

public interface IInspectorPropertyDrawer
{
    UIContent Content { get; private protected set; }
    object Target { get; internal set; }
    MemberData MemberData { get; internal set; }
    internal void InitInternal();
    internal void TickInternal();
    internal void Draw(Rectangle bounds);
}

/// <summary>
///
/// </summary>
///
/// <remarks>
/// Subclasses should have a constructor taking no arguments
/// </remarks>
public abstract class InspectorPropertyDrawer<TFor> : IInspectorPropertyDrawer
{
    protected Log log { get; } = new Log($"PropertyDrawer for {typeof(TFor).Name}");
    public UIContent Content { get; set; }
    public MemberData MemberData { get; set; }
    public object Target { get; set; }
    protected TrackedValue TrackedValue { get; private set; }

    void IInspectorPropertyDrawer.InitInternal()
    {
        TrackedValue = new TrackedValue(Target, MemberData.Name);
        Content = CreateContent();
        Init();
    }

    protected abstract UIContent CreateContent();
    internal protected abstract void Init();
    internal protected virtual void Tick() { }

    void IInspectorPropertyDrawer.TickInternal()
    {
        Tick();
        if (TrackedValue.HasValueChanged())
            ValueUpdated();
    }

    internal protected abstract void ValueUpdated();

    void IInspectorPropertyDrawer.Draw(Rectangle bounds)
    {
        Content?.InternalDraw(bounds);
    }
}

public abstract class InspectorPropertyDrawer<UIContentType, TFor> : InspectorPropertyDrawer<TFor>
    where UIContentType : UIContent
{
    public new UIContentType Content => (UIContentType)base.Content;
}