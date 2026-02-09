using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;
internal static class UIWindowManager
{
    private const int PRIORITY_BASE = 100000;
    private const int PRIORITY_RANGE = 10000;

    private static List<UIWindow> windows = new List<UIWindow>();

    internal static void Update()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            if (!windows[i].IsClosing)
                windows[i].UpdateContainer();

            if (windows[i].IsFullyClosed)
            {
                InputManager.UnregisterContext(windows[i].Input);
                windows.RemoveAt(i);              
                i--;

            }
        }

        for (int i = windows.Count - 1; i >= 0; i--)
        {
            UIWindow w = windows[i];
            if (w.IsClosing)
                continue;

            if (w.Input.IsPressed(MouseButton.Left))
            {
                BringToFront(w);
                break;
            }
        }
    }

    internal static void Draw()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].Draw();
        }
    }

    internal static void Show(UIWindow uiWindow)
    {
        if(!windows.Contains(uiWindow))
        {
            InputManager.RegisterContext(uiWindow.Input);
            windows.Add(uiWindow);
        }
        else
        {
            // TODO: Make border of window flash red
        }

        ForgeWardenEngine.Current.GlobalThreadLoom.InvokeAfter(
            ForgeWardenEngine.ENGINE_POOL_NAME, 
            () => BringToFront(uiWindow), 
            TimeSpan.FromMilliseconds(100));
    }

    internal static void BringToFront(UIWindow window)
    {
        int idx = windows.IndexOf(window);
        if (idx < 0) return;

        if (idx != windows.Count - 1)
        {
            windows.RemoveAt(idx);
            windows.Add(window);
        }

        ReassignPriorities();
    }

    private static void ReassignPriorities()
    {
        int count = windows.Count;
        // assign priorities starting at PRIORITY_BASE up to PRIORITY_BASE + PRIORITY_RANGE - 1
        // if count exceeds PRIORITY_RANGE we wrap around (keeps relative ordering but reuses the block)
        for (int i = 0; i < count; i++)
        {
            int priority = PRIORITY_BASE + (i % PRIORITY_RANGE);
            windows[i].Input.Priority = priority;
        }
    }
}
