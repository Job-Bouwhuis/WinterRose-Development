using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ImGuiApps;

namespace WinterRose.ImGuiApps;

public class LoadingAscii
{
    public static bool ShowDebug = false;

    private string loadinganimation = "|/-\\";
    private int loadinganimationindex = 0;
    private float nextframeTime = 0;
    private readonly float nextFrameTimeMax = 600000000000;

    DateTime last;

    public void Render()
    {
        if (ShowDebug)
        {
            gui.Text(nextFrameTimeMax.ToString());
            gui.Text(Time.DeltaTime.ToString());
            gui.Text(nextframeTime.ToString());
        }

        gui.Text(loadinganimation[loadinganimationindex].ToString());

        if (nextframeTime > nextFrameTimeMax)
        {
            loadinganimationindex += 1;
            if (loadinganimationindex >= loadinganimation.Length)
            {
                loadinganimationindex = 0;
            }
            nextframeTime = 0;
        }
        else
        {
            float delta = (float)(DateTime.Now - last).TotalSeconds;
            nextframeTime += delta;
        }
    }
}
