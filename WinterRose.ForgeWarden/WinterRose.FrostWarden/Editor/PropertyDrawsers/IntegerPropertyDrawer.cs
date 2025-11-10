using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.Editor;

public class IntegerPropertyDrawer : InspectorPropertyDrawer<UINumericControlBase<int>, int>
{
    protected override UIContent CreateContent()
    {
        if (MemberData.HasAttribute<AsSliderAttribute>() && MemberData.CanWrite)
            return new UIValueSlider<int>();

        var updown= new UINumericUpDown<int>();
        if (!MemberData.CanWrite)
            updown.ReadOnly = true;

        return updown;
    }

    protected internal override void Init()
    {
        Content.Label = MemberData.Name;
        Content.SetValue((int?)TrackedValue.Value ?? 0, false);
        if (MemberData.CanWrite)
            Content.ValueChanged.Subscribe(OnChanged);
    }

    private void OnChanged(double b)
    {
        TrackedValue.Set((int)b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetValue((int?)TrackedValue.Value ?? 0, false);
    }
}