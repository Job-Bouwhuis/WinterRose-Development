using BulletSharp.SoftBody;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRoseUtilityApp.SystemMonitor;

internal static class ContainerCreators
{


    internal static UIWindow SystemMonitor()
    {
        UIWindow window = new UIWindow("System Monitor", 700, 1000);

        window.AddContent(new UIText("System Resource Overview", UIFontSizePreset.Title));

        var sys = SystemMonitorEntry.Instance;

        // CPU
        window.AddContent(new UIText("CPU", UIFontSizePreset.Subtitle));

        UIGraph cpuGraph = new();

        window.AddContent(cpuGraph);
        cpuGraph.MaxDataPoints = 40;

        double now = Raylib.GetTime();

        UIInvocationContent cpuUpdater = new();
        cpuUpdater.OnUpdate = Invocation.Create(() =>
        {
            if(Raylib.GetTime() - now < 0.5)
                return;
            now = Raylib.GetTime();

            var usage = sys.GetCpuUsage();
            cpuGraph.AddValueToSeries("CPU Usage", usage.AverageUsage ?? 0, Color.Blue);

            var info = sys.GetCpuTemperature();
            cpuGraph.AddValueToSeries("CPU Temperature", info.CoreTemperatures.Sum(), Color.Orange);
        });
        window.AddContent(cpuUpdater);

        return window;
    }
}
