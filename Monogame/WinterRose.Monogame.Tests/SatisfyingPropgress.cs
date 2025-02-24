using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gui = ImGuiNET.ImGui;

namespace WinterRose.Monogame.Tests;

internal class SatisfyingPropgress : ImGuiLayout
{
    float progress = 0f;
    float increment = 0.001f;

    public override void RenderLayout()
    {
        gui.Begin("Satisfying Progress");
        if(gui.Button("Reset"))
        {
            progress = 0f;
        }

        foreach(int i in 100)
        {
            gui.ProgressBar(Clamp(progress + increment * i, 0, 1), new System.Numerics.Vector2(gui.GetContentRegionAvail().X, 25), "");
        }
        progress += increment;
        System.Threading.Thread.Sleep(10);
        gui.End();
    }

    protected override void Update() { }

    private float Clamp(float v1, float v2, float v3)
    {
        if(v1 < v2)
        {
            return v2;
        }
        else if(v1 > v3)
        {
            return v3;
        }
        else
        {
            return v1;
        }
    }
}
