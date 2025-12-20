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
            private static extern bool GetSystemPowerStatus(out SystemPowerInfo lpSystemPowerStatus);

            public static SystemPowerInfo GetPowerStatus()
            {
                if (OperatingSystem.IsWindows())
                {
                    if (GetSystemPowerStatus(out SystemPowerInfo status))
                        return status;
                    throw new InvalidOperationException("Unable to get power status");
                }
                else if (OperatingSystem.IsLinux())
                {
                    return GetLinuxPowerStatus();
                }
                else
                {
                    throw new PlatformNotSupportedException("Only Windows and Linux are supported");
                }
            }

            private static SystemPowerInfo GetLinuxPowerStatus()
            {
                string basePath = "/sys/class/power_supply/";
                string batteryPath = null;

                // Try to find a battery directory
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    if (dir.Contains("BAT"))
                    {
                        batteryPath = dir;
                        break;
                    }
                }

                var status = new SystemPowerInfo
                {
                    ACLineStatus = ACLineStatus.Unknown,
                    BatteryFlag = BatteryFlag.Unknown,
                    BatteryLifePercent = 255,
                    Reserved1 = 0,
                    BatteryLifeTime = uint.MaxValue,
                    BatteryFullLifeTime = uint.MaxValue
                };

                if (batteryPath == null)
                {
                    // No battery, assume AC only
                    status.ACLineStatus = ACLineStatus.Online;
                    status.BatteryFlag = BatteryFlag.NoBattery;
                    status.BatteryLifePercent = 100;
                    return status;
                }

                try
                {
                    // AC line status
                    string acPath = Path.Combine(basePath, "AC", "online");
                    if (File.Exists(acPath))
                    {
                        status.ACLineStatus = File.ReadAllText(acPath).Trim() == "1" ? ACLineStatus.Online : ACLineStatus.Offline;
                    }

                    // Battery percentage
                    string capacityFile = Path.Combine(batteryPath, "capacity");
                    if (File.Exists(capacityFile))
                    {
                        status.BatteryLifePercent = byte.Parse(File.ReadAllText(capacityFile).Trim());
                    }

                    // Battery charging status
                    string statusFile = Path.Combine(batteryPath, "status");
                    if (File.Exists(statusFile))
                    {
                        string batteryStatus = File.ReadAllText(statusFile).Trim().ToLower();
                        if (batteryStatus.Contains("charging"))
                            status.BatteryFlag = BatteryFlag.Charging;
                        else if (batteryStatus.Contains("discharging"))
                        {
                            // Set Low or Critical based on capacity
                            if (status.BatteryLifePercent <= 5)
                                status.BatteryFlag = BatteryFlag.Critical;
                            else if (status.BatteryLifePercent <= 20)
                                status.BatteryFlag = BatteryFlag.Low;
                            else
                                status.BatteryFlag = BatteryFlag.High;
                        }
                        else if (batteryStatus.Contains("full"))
                            status.BatteryFlag = BatteryFlag.High;
                        else
                            status.BatteryFlag = BatteryFlag.Unknown;
                    }

                    // Linux sysfs does not provide lifetime in seconds, keep as MaxValue
                    status.BatteryLifeTime = uint.MaxValue;
                    status.BatteryFullLifeTime = uint.MaxValue;
                }
                catch
                {
                    // Fallback to Unknown
                    status.ACLineStatus = ACLineStatus.Unknown;
                    status.BatteryFlag = BatteryFlag.Unknown;
                    status.BatteryLifePercent = 255;
                    status.BatteryLifeTime = uint.MaxValue;
                    status.BatteryFullLifeTime = uint.MaxValue;
                }

                return status;
            }
        }
    }
}
