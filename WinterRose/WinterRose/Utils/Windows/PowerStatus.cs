using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        /// <summary>
        /// A class that provides information about the power status of the system.
        /// </summary>
        public static class PowerStatus
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetSystemPowerStatus(out SystemPowerInfo lpSystemPowerStatus);

            public static SystemPowerInfo GetPowerStatus()
            {
                if (GetSystemPowerStatus(out SystemPowerInfo status))
                {
                    return status;
                }
                else
                {
                    throw new InvalidOperationException("Unable to get power status");
                }
            }
        }
    }
}
