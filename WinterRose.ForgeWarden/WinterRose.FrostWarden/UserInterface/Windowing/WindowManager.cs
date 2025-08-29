using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;
internal static class WindowManager
{
    // Reserve a priority block for the windowing system.
    // You can increase PRIORITY_RANGE easily if you need more slots.
    private const int PRIORITY_BASE = 100000;
    private const int PRIORITY_RANGE = 10000;

    private static List<UIWindow> windows = new List<UIWindow>();

    internal static void Update()
    {
        // first update all windows so their input states are refreshed
        for (int i = 0; i < windows.Count; i++)
        {
            if (!windows[i].IsClosing)
                windows[i].UpdateContainer();

            if (windows[i].IsFullyClosed)
            {
                windows.RemoveAt(i);
                i--;
            }
        }

        // then check input from top-most to bottom-most so top windows get precedence when deciding
        for (int i = windows.Count - 1; i >= 0; i--)
        {
            UIWindow w = windows[i];
            if (w.IsClosing)
                continue;

            // If user left-clicked this window (Input.IsDown only returns true when the input system allowed it),
            // bring it to front so it is drawn last and receives highest priority.
            if (w.Input.IsPressed(MouseButton.Left))
            {
                BringToFront(w);
                break; // one click => one window receives focus
            }
        }
    }

    internal static void Draw()
    {
        // draw in list order; windows at the end are drawn last (top-most)
        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].Draw();
        }
    }

    internal static void Show(UIWindow uIWindow)
    {
        if(!windows.Contains(uIWindow))
        {
            windows.Add(uIWindow);
        }
        else
        {
            // TODO: Make border of window flash red
        }
        BringToFront(uIWindow);
    }

    internal static void BringToFront(UIWindow window)
    {
        int idx = windows.IndexOf(window);
        if (idx < 0) return;

        // if already top-most, still ensure priorities are consistent
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
