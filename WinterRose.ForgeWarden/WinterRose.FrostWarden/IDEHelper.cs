namespace WinterRose.ForgeWarden;

using EnvDTE;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class IDEHelper
{
    public static bool OpenFileAt(string filePath, int line)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            var dte = GetActiveVisualStudioInstance();
            if (dte == null)
                return false;

            dte.MainWindow.Activate();
            dte.ItemOperations.OpenFile(filePath);
            dte.ExecuteCommand("Edit.GoTo", line.ToString());
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("IDEHelper failed: " + ex.Message);
            return false;
        }
    }

    private static DTE GetActiveVisualStudioInstance()
    {
        IRunningObjectTable rot;
        IEnumMoniker enumMoniker;
        GetRunningObjectTable(0, out rot);
        rot.EnumRunning(out enumMoniker);
        enumMoniker.Reset();

        IntPtr fetched = IntPtr.Zero;
        IMoniker[] moniker = new IMoniker[1];
        while (enumMoniker.Next(1, moniker, fetched) == 0)
        {
            IBindCtx ctx;
            CreateBindCtx(0, out ctx);

            string displayName;
            moniker[0].GetDisplayName(ctx, null, out displayName);

            // Typical ROT name example:
            // "!VisualStudio.DTE.17.0:12345"
            if (!displayName.StartsWith("!VisualStudio.DTE", StringComparison.OrdinalIgnoreCase))
                continue;

            rot.GetObject(moniker[0], out object comObject);
            if (comObject is DTE dte)
                return dte;
        }
        return null;
    }

    [DllImport("ole32.dll")]
    private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

    [DllImport("ole32.dll")]
    private static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable pprot);
}