using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.Editor;

public class BooleanPropertyDrawer : InspectorPropertyDrawer<UICheckBox, bool>
{
    protected override UIContent CreateContent()
    {
        return new UICheckBox();
    }

    protected internal override void Init()
    {
        Content.Text = MemberData.Name;
        Content.SetChecked((bool?)TrackedValue.Value ?? false);
        if (!MemberData.CanWrite)
            Content.Disabled = true;
        else
            Content.OnCheckedChangedBasic.Subscribe(OnChanged);
    }

    private void OnChanged(bool b)
    {
        TrackedValue.Set(b);
    }

    protected internal override void ValueUpdated()
    {
        Content.SetChecked((bool?)TrackedValue.Value ?? false);
    }
}