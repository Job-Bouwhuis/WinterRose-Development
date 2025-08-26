using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks.Exceptions;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;
public class UIWindow : UIContainer
{
    private InputContext input = new(new RaylibInputProvider(), -1, false);
    public override InputContext Input => input;

    public Vector2 Position
    {
        get => CurrentPosition.Position;
        set => CurrentPosition.Position = TargetPosition = value;
    }

    public Vector2 Size
    {
        get => CurrentPosition.Size;
        set => CurrentPosition.Size = TargetSize = value;
    }

    public new WindowStyle Style
    {
        get => (WindowStyle)base.Style;
        set => base.Style = value;
    }

    public override float Height => CurrentPosition.Size.Y + base.Height;

    public UIWindow(float width, float height)
    {
        Style = new WindowStyle();
        Size = new Vector2(width, height);
    }

    public void Show()
    {
        WindowManager.Show(this);
    }

    protected internal override void DrawContainer() => base.DrawContainer();

    protected override void AlterBoundsCorrectlyForDragBar(ref Rectangle backgroundBounds, float dragHeight)
    {

    }
}
