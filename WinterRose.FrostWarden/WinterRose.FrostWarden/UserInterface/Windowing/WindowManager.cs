using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;
internal static class WindowManager
{
    private static List<UIWindow> windows = [];

    internal static void Update()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].UpdateContainer();
        }
    }

    internal static void Draw()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].DrawContainer();
        }
    }

    internal static void Show(UIWindow uIWindow)
    {
        windows.Add(uIWindow);
    }
}
