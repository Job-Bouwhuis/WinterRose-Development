using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public class UIFPS : UIContent
{
    UIText text = new("");

    public UIFontSizePreset Preset
    {
        get => text.Preset;
        set => text.Preset = value;
    }

    protected internal override void Setup()
    {
        text.owner = owner;
    }

    public override Vector2 GetSize(Rectangle availableArea) => text.GetSize(availableArea);
    protected override void Draw(Rectangle bounds)
    {
        text.InternalDraw(bounds);
    }
    protected internal override float GetHeight(float maxWidth)
    {
        return text.GetHeight(maxWidth);
    }

    protected internal override void Update()
    {
        int fps = ray.GetFPS();

        
        string colorPrefix = fps switch
        {
            > 120 => @"\c[#00FF00]",   // bright green
            > 90 => @"\c[#80FF00]",   // yellow-green
            > 40 => @"\c[#FFFF00]",   // yellow
            > 20 => @"\c[#FF8000]",   // orange
            > 5 => @"\c[#FF4000]",   // reddish-orange
            <= 5 => @"\c[#FF0000]",   // bright red
        };

        text.Text = colorPrefix + fps.ToString();
    }
}
