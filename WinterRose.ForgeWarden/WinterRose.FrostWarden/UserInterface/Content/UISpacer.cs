using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface;
public class UISpacer : UIContent
{
    public float SpaceAmount { get; }

    public bool DrawSeparator { get; set; } = true;

    public UISpacer(float spaceAmount, bool drawSeparator = true)
    {
        SpaceAmount = spaceAmount;
        DrawSeparator = drawSeparator;
    }

    public UISpacer() => SpaceAmount = 30;

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new(availableArea.Width, SpaceAmount);
    }

    protected internal override float GetHeight(float maxWidth) => SpaceAmount;

    protected override void Draw(Rectangle bounds)
    {
        if (!DrawSeparator)
            return;

        float y = bounds.Y + (bounds.Height / 2f);

        Raylib.DrawLine(
            (int)bounds.X,
            (int)y,
            (int)(bounds.X + bounds.Width),
            (int)y,
            Style.SeperatorLineColor
        );
    }

}
