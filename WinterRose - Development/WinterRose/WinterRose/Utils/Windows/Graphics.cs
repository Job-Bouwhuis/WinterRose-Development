using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Vectors;
using G = System.Drawing.Graphics;

namespace WinterRose
{
    public static partial class Windows
    {
        /// <summary>
        /// Used to draw directly to the screen, it is not recommended to use this class as it will cause flickering and other rendering issues regarding rendering of anything else.
        /// </summary>
        public partial class Graphics
        {
            [LibraryImport("user32.dll")]
            private static partial IntPtr GetDC(IntPtr hwnd);

            [LibraryImport("user32.dll")]
            private static partial int ReleaseDC(IntPtr hwnd, IntPtr hdc);


            public static void DrawLine(Vector2I pos1, Vector2I pos2)
            {
                nint hdc = GetDC(IntPtr.Zero);

                if(hdc == IntPtr.Zero)
                    return;

                G graphics = G.FromHdc(hdc);
                graphics.DrawLine(Pens.Red, (Point)pos1, (Point)pos2);

                graphics.Dispose();
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
    }
}
