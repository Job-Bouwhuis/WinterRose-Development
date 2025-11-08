using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.Editor;

public class DoublePropertyDrawer : InspectorPropertyDrawer<NumericControlBase<double>, double>
{
    protected override UIContent CreateContent()
    {
        if (MemberData.HasAttribute<AsSliderAttribute>() && MemberData.CanWrite)
            return new UIValueSlider<double>()
            {
                Step = 0.1
            };

        var updown = new UINumericUpDown<double>();
        if (!MemberData.CanWrite)
            updown.ReadOnly = true;

        return updown;
    }

    protected internal override void Init()
    {
        Content.Label = MemberData.Name;
        Content.SetValue((double?)TrackedValue.Value ?? 0, false);
        if (MemberData.CanWrite)
            Content.ValueChanged.Subscribe(OnChanged);
    }

    private void OnChanged(double b)
    {
        TrackedValue.Set(b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetValue((double?)TrackedValue.Value ?? 0, false);
    }
}

public class Vector3PropertyDrawer : InspectorPropertyDrawer<Vector3>
{
    private UINumericUpDown<float> x;
    private UINumericUpDown<float> y;
    private UINumericUpDown<float> z;

    protected override UIContent CreateContent()
    {
        UIColumns cols = new();
        x =  new UINumericUpDown<float>();
        x.Label = "x";

        y =  new UINumericUpDown<float>();
        y.Label = "y";

        z =  new UINumericUpDown<float>();
        z.Label = "z";
        return cols;
    }

    protected internal override void Init()
    {
        ref Vector3 val = ref TrackedValue.GetValueRef<Vector3>();
    }

    protected internal override void ValueUpdated()
    {

    }
}