using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.SystemMonitor;

[SubSystemSkip]
internal class SystemMonitorEntry : SubSystem
{
    readonly Dictionary<string, RollingAverage> cpuUsageAverages = new();
    readonly RollingAverage totalCpuAverage = new(20); // ~20 samples = about 1s if you poll every 50ms

    private class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var sub in hardware.SubHardware)
                sub.Accept(this);
        }
        public void VisitSensor(ISensor sensor) 
        {
            if(sensor.Hardware.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature)
                Console.WriteLine(sensor.Value ?? -404);
        }
        public void VisitParameter(IParameter parameter) { }
    }

    public static SystemMonitorEntry Instance { get; private set; }

    private readonly Computer computer;
    private readonly UpdateVisitor visitor = new();

    public IReadOnlyList<IHardware> HardwareList { get; private set; }

    public SystemMonitorEntry()
        : base("System Monitor", "Watches over the system resources and alerts on dangerous values", new Version(1, 0, 0))
    {
        Instance = this;

        computer = new Computer
        {
            IsCpuEnabled = true,
            //IsMemoryEnabled = true,
            //IsGpuEnabled = true,
            //IsMotherboardEnabled = true,
            //IsStorageEnabled = true,
            //IsNetworkEnabled = true
        };

        Program.Current.AddTrayItem(new UIButton("Show system stats", (c, b) =>
        {
            ContainerCreators.SystemMonitor().Show();
        }));
    }

    public override void Init()
    {
        computer.Open();

        foreach (var hardware in computer.Hardware)
        {
            Console.WriteLine($"[HW] {hardware.Name} ({hardware.HardwareType})");

            foreach (var sensor in hardware.Sensors)
                Console.WriteLine($"  [SENSOR] {sensor.SensorType} {sensor.Name} = {sensor.Value}");

            foreach (var sub in hardware.SubHardware)
            {
                Console.WriteLine($"  [SUBHW] {sub.Name}");
                foreach (var sensor in sub.Sensors)
                    Console.WriteLine($"    [SENSOR] {sensor.SensorType} {sensor.Name} = {sensor.Value}");
            }
        }

        // filter out integrated graphics
        HardwareList = computer.Hardware
            .Where(h =>
                h.HardwareType != HardwareType.GpuIntel &&
                h.HardwareType != HardwareType.SuperIO)
            .ToList();
    }

    public override void Update()
    {
        foreach (IHardware hardware in HardwareList)
        {
            hardware.Update();
            foreach (IHardware sub in hardware.SubHardware)
                sub.Update();
        }
    }

    public override void Destroy()
    {
        computer.Close();
    }

    public struct CpuUsageInfo
    {
        public string[] CpuNames;
        public float? AverageUsage;
        public float[] CoreUsages;
        public string[] CoreNames;
    }
    public struct CpuTemperatureInfo
    {
        public string[] CpuNames;
        public float? AverageTemperature;
        public float[] CoreTemperatures;
        public string[] CoreNames;
    }

    // inside SystemMonitorEntry
    private const int SAMPLE_COUNT_TEMPS = 30;
    private readonly Dictionary<string, Queue<float>> temperatureSamples = new();

    public CpuTemperatureInfo GetCpuTemperature()
    {
        var cpuHardware = computer.Hardware
            .Where(h => h.HardwareType == HardwareType.Cpu)
            .ToArray();

        if (cpuHardware.Length == 0)
        {
            return new CpuTemperatureInfo
            {
                CpuNames = Array.Empty<string>(),
                AverageTemperature = null,
                CoreTemperatures = Array.Empty<float>(),
                CoreNames = Array.Empty<string>()
            };
        }

        List<string> cpuNames = cpuHardware.Select(h => h.Name).ToList();
        List<float> averagedTemps = new();
        List<string> names = new();

        foreach (var cpu in cpuHardware)
        {
            UpdateRecursive(cpu);

            ISensor[] tempSens = cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature).ToArray();

            foreach (var sensor in tempSens)
            {
                if (sensor.Value.HasValue)
                {
                    string key = $"{cpu.Name}:{sensor.Name}";
                    float value = sensor.Value.Value;

                    if (!temperatureSamples.TryGetValue(key, out var samples))
                    {
                        samples = new Queue<float>();
                        temperatureSamples[key] = samples;
                    }

                    samples.Enqueue(value);
                    if (samples.Count > SAMPLE_COUNT_TEMPS)
                        samples.Dequeue();

                    averagedTemps.Add(samples.Average());
                    names.Add(sensor.Name);
                }
            }
        }

        float? avg = averagedTemps.Count > 0 ? averagedTemps.Average() : null;

        return new CpuTemperatureInfo
        {
            CpuNames = cpuNames.ToArray(),
            AverageTemperature = avg,
            CoreTemperatures = averagedTemps.ToArray(),
            CoreNames = names.ToArray()
        };
    }

    private void UpdateRecursive(IHardware hardware)
    {
        hardware.Update();
        foreach (var sub in hardware.SubHardware)
            UpdateRecursive(sub);
    }
    public CpuUsageInfo GetCpuUsage()
    {
        var cpuHardware = computer.Hardware
            .Where(h => h.HardwareType == HardwareType.Cpu)
            .ToArray();

        if (cpuHardware.Length == 0)
            return new CpuUsageInfo
            {
                CpuNames = Array.Empty<string>(),
                AverageUsage = null,
                CoreUsages = Array.Empty<float>(),
                CoreNames = Array.Empty<string>()
            };

        List<string> cpuNames = cpuHardware.Select(h => h.Name).ToList();
        List<float> usages = new();
        List<string> names = new();
        float? total = null;

        foreach (var cpu in cpuHardware)
        {
            cpu.Update();
            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Load && sensor.Value.HasValue)
                {
                    string sensorName = sensor.Name;

                    if (sensorName.Equals("CPU Total", StringComparison.OrdinalIgnoreCase))
                    {
                        totalCpuAverage.AddSample(sensor.Value.Value);
                        total = totalCpuAverage.GetAverage();
                    }
                    else if (sensorName.Contains("Core", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!cpuUsageAverages.TryGetValue(sensorName, out var avg))
                        {
                            avg = new RollingAverage(50);
                            cpuUsageAverages[sensorName] = avg;
                        }

                        avg.AddSample(sensor.Value.Value);
                        usages.Add(avg.GetAverage());
                        names.Add(sensorName);
                    }
                }
            }
        }

        return new CpuUsageInfo
        {
            CpuNames = cpuNames.ToArray(),
            AverageUsage = total,
            CoreUsages = usages.ToArray(),
            CoreNames = names.ToArray()
        };
    }


    public float? GetGpuTemperature()
    {
        var gpu = HardwareList.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd);
        if (gpu == null) return null;

        var sensor = gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
        return sensor?.Value;
    }

    public float? GetGpuUsage()
    {
        var gpu = HardwareList.FirstOrDefault(h => h.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd);
        if (gpu == null) return null;

        var sensor = gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase));
        return sensor?.Value;
    }

    public float? GetMemoryUsage()
    {
        return FindSensor(HardwareType.Memory, SensorType.Load, "Memory");
    }

    private float? FindSensor(HardwareType type, SensorType sensorType, string nameMatch)
    {
        var hardware = HardwareList.FirstOrDefault(h => h.HardwareType == type);
        if (hardware == null) return null;

        var sensor = hardware.Sensors.FirstOrDefault(s =>
            s.SensorType == sensorType &&
            s.Name.Contains(nameMatch, StringComparison.OrdinalIgnoreCase));

        return sensor?.Value;
    }
}