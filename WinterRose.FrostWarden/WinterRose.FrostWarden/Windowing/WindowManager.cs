using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Windowing
{
    public static class WindowManager
    {
        private static readonly List<Window> windows = new();

        public static void Add(Window window)
        {
            windows.Add(window);
        }

        public static void UpdateAll()
        {
        }

        public static void DrawAll()
        {
        }

        public static void CloseAll()
        {
            foreach (var window in windows)
                window.Close();
            windows.Clear();
        }
    }
}
