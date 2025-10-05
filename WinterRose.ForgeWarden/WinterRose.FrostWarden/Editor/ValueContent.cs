using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.Editor;
internal class ValueContent : UIContent
{
    public required UIContent graphic;
    public TrackedValue trackedValue;

    public override Vector2 GetSize(Rectangle availableArea) => graphic.GetSize(availableArea);
    protected override void Draw(Rectangle bounds) => graphic.InternalDraw(bounds);
    protected internal override float GetHeight(float maxWidth) => graphic.GetHeight(maxWidth);
}
