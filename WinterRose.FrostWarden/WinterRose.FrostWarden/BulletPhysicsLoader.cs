using System.Reflection;
using System.Runtime.InteropServices;

namespace WinterRose.FrostWarden;

internal static class BulletPhysicsLoader
{
    private static readonly string LIB_DIR = Path.Combine(AppContext.BaseDirectory, "runtimes/native");
    private static readonly string CONFIG_NAME = "BulletSharp.dll.config";

    public static void LoadBulletLibrary()
    {
        string outputPath = Path.Combine(LIB_DIR, GetPlatformFileName());
        NativeLibrary.Load(outputPath);
    }

    public static bool TryLoadBulletSharp()
    {
        try
        {
            ExtractBulletDependencies();
            LoadBulletLibrary();
            return true;
        }
        catch (Exception e)
        {
            Windows.MessageBox(e.ToString(), "Error loading BulletSharp", Windows.MessageBoxButtons.OK, Windows.MessageBoxIcon.Error);
            return false;
        }
    }

    private static void ExtractBulletDependencies()
    {
        Directory.CreateDirectory(LIB_DIR);

        // Extract the platform-specific DLL
        string resourceName = GetPlatformResourceName();
        string outputPath = Path.Combine(LIB_DIR, GetPlatformFileName());

        using var dllStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (dllStream == null)
            throw new Exception($"Embedded native library '{resourceName}' not found.");

        using var dllFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        dllStream.CopyTo(dllFile);
    }

    private static string GetPlatformFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "libbulletc.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "libbulletc.so";
        }

        throw new PlatformNotSupportedException("Unsupported platform.");
    }

    private static string GetPlatformResourceName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            bool is64 = RuntimeInformation.OSArchitecture == Architecture.X64;
            return $"WinterRose.FrostWarden.libbulletc-windows-{(is64 ? "x64" : "x86")}.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "WinterRose.FrostWarden.libbulletc-linux-x64.so";
        }

        throw new PlatformNotSupportedException("Unsupported platform.");
    }
}