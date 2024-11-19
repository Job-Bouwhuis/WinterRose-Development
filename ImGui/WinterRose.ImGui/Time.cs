using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps;

public static class Time
{
    /// <summary>
    /// The time since the last frame in seconds.
    /// </summary>
    public static float DeltaTime => gui.GetIO().DeltaTime;
    /// <summary>
    /// The time since the application started in seconds.
    /// </summary>
    public static float TimeSinceStart => (float)watch.Elapsed.TotalSeconds;

    private static Stopwatch watch = new();

    static Time()
    {
        watch.Start();
    }
}
