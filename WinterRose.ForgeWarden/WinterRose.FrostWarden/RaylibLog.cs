using Microsoft.VisualStudio.OLE.Interop;
using Raylib_cs;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinterRose.ForgeWarden.Entities;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden;

internal unsafe static class RaylibLog
{
    static Log rayLog;
    static delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void> logCallbackPtr = &raylibLogMethod;

    private unsafe struct rayLogEntry
    {
        public int logLevel;
        public string msg;
        public byte[] data;
    }

    private static readonly ConcurrentQueue<rayLogEntry> rayLogQueue = new();
    private static readonly AutoResetEvent queueNotifier = new(false);
    private static Thread? logThread;
    private static bool closing;

    internal static void Setup()
    {
        rayLog = new Log("Ray");
        ray.SetTraceLogCallback(logCallbackPtr);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void raylibLogMethod(int logLevel, sbyte* s1, sbyte* s2)
    {
        string s = Logging.GetLogMessage((nint)s1, (nint)s2);
        LogEntry logEntry = new LogEntry(MapRaylibSeverity(logLevel), rayLog.Category, s, "", -1, -1);
        rayLog.Write(logEntry);
    }

    private static LogSeverity MapRaylibSeverity(int level) => level switch
    {
        0 or 1 or 2 => LogSeverity.Debug,
        3 => LogSeverity.Info,
        4 => LogSeverity.Warning,
        5 => LogSeverity.Error,
        6 => LogSeverity.Fatal,
        _ => LogSeverity.Info
    };
}