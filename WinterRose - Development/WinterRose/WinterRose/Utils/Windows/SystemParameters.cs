using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static class SystemParameters
    {
        // Constants for SystemParametersInfo
        private const int SPI_SETCURSORS = 0x0057;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;
        private const int SPI_GETCURRENTPADDING = 0x0076;

        public static int GetCursorSize()
        {
            // Variable to store the cursor size
            int cursorSize;

            // Call SystemParametersInfo to get the cursor size
            if (SystemParametersInfo(SPI_GETCURRENTPADDING, 0, out cursorSize, 0))
            {
                // Successfully retrieved cursor size
                return cursorSize;
            }
            else
            {
                // Failed to get cursor size
                // You may want to handle this error condition appropriately
                throw new Exception("Failed to get cursor size.");
            }
        }

        public static bool SetCursorSize(int newSize)
        {
            // Call SystemParametersInfo to set cursor size
            return SystemParametersInfo(SPI_SETCURSORS, 0, newSize, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }


        // Import the SystemParametersInfo function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        // Import the SystemParametersInfo function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, out int pvParam, int fWinIni);
    }
}
