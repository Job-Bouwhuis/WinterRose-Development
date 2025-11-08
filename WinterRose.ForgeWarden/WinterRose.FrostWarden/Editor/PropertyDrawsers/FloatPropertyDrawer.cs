using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.Editor;

public class FloatPropertyDrawer : InspectorPropertyDrawer<NumericControlBase<float>, float>
{
    protected override UIContent CreateContent()
    {
        if (MemberData.HasAttribute<AsSliderAttribute>() && MemberData.CanWrite)
            return new UIValueSlider<float>()
            {
                Step = 0.1f
            };

        var updown = new UINumericUpDown<float>();
        if (!MemberData.CanWrite)
            updown.ReadOnly = true;

        return updown;
    }

    protected internal override void Init()
    {
        Content.Label = MemberData.Name;
        Content.SetValue((float?)TrackedValue.Value ?? 0, false);
        if (MemberData.CanWrite)
            Content.ValueChanged.Subscribe(OnChanged);
    }

    private void OnChanged(double b)
    {
        TrackedValue.Set((float)b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetValue((float?)TrackedValue.Value ?? 0, false);
    }
}