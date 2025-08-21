using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface;
internal class UISpacer : UIContent
{
    public float SpaceAmount { get; }

    public UISpacer(float spaceAmount)
    {
        SpaceAmount = spaceAmount;
    }

    public UISpacer() => SpaceAmount = 30;

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new(availableArea.Width, SpaceAmount);
    }

    protected internal override float GetHeight(float maxWidth) => SpaceAmount;

    protected override void Draw(Rectangle bounds) { }
}
