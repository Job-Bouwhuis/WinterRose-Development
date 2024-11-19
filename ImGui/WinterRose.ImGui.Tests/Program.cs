using gui = ImGuiNET.ImGui;
using col = ImGuiNET.ImGuiCol;
using ImGuiNET;
using System.Numerics;

namespace WinterRose.ImGuiApps.Tests;

class Program : Application
{
    static void Main(string[] args)
    {
        RegPrefs.Flush();
        using var app = new Program();
        var wind = new TestWindow();
        app.AddWindow(wind);

        app.Run().Wait();
    }
}