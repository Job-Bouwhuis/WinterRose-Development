using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.Editor;

internal class NoCustomDrawer() : InspectorPropertyDrawer<UIText>
{
    protected override UIContent CreateContent()
    {
        return new UIText("");
    }

    protected internal override void Init() =>
        ((UIText)Content).Text = MemberData.Name + " = " + MemberData.GetValue(Target)?.ToString() ?? "null";
    protected internal override void ValueUpdated() =>
        ((UIText)Content).Text = MemberData.Name + " = " + MemberData.GetValue(Target)?.ToString() ?? "null";
}