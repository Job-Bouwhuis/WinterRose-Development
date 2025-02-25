using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        public static List<WindowsHandle> GetChildren()
        {
            Process process = Process.GetCurrentProcess();

            List<WindowsHandle> children = [];

            foreach (ProcessThread processThread in process.Threads)
            {
                EnumThreadWindows(processThread.Id,
                 (hWnd, lParam) =>
                 {
                     //Check if Window is Visible or not.
                     if (!IsWindowVisible((int)hWnd))
                         return true;

                     //Get the Window's Title.
                     StringBuilder title = new StringBuilder(256);
                     GetWindowText((int)hWnd, title, 256);

                     //Check if Window has Title.
                     if (title.Length == 0)
                         return true;

                     WindowsHandle handle = new WindowsHandle(title.ToString(), hWnd);
                     children.Add(handle);

                     return true;
                 }, IntPtr.Zero);
            }

            return children;
        }
    }
}
