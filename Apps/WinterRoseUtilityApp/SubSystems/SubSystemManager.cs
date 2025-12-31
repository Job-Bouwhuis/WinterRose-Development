using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRoseUtilityApp.SubSystems;

public class SubSystemManager
{
    private Log log = new Log("Subsystem Manager");

    private List<SubSystem> subSystems = new List<SubSystem>();
    public IReadOnlyList<SubSystem> SubSystems => subSystems;

    public bool Initialize([NotNullWhen(false)] out Exception? exception)
    {
        try
        {
            Type[] types = TypeWorker.FindTypesWithBase<SubSystem>();

            foreach (Type t in types)
            {
                if (t == typeof(SubSystem))
                    continue;
                if (Attribute.IsDefined(t, typeof(SubSystemSkipAttribute)))
                {
                    log.Info($"Skipping subsystem '{t.Name}' due to SubSystemSkip Attribute");
                    continue;
                }
                log.Info($"Initializing subsystem '{t.Name}'");
                var stopwatch = Stopwatch.StartNew();
                SubSystem subsys;
                try
                {
                    subsys = (SubSystem)DynamicObjectCreator.CreateInstance(t, []);
                }
                catch (Exception ex)
                {
                    log.Critical(ex, "Failed to start subsystem " + t.Name);
                    continue;
                }

                stopwatch.Stop();
                log.Info($"Took {stopwatch.Elapsed.TotalMilliseconds}ms");
                if (subsys is null)
                {
                    log.Error($"Failed to start subsystem {t.Name}");
                    continue;
                }
                subSystems.Add(subsys);
            }

            foreach (SubSystem subSystem in subSystems)
            {
                try
                {
                    subSystem.Init();
                }
                catch (Exception ex)
                {
                    log.Critical(ex, "Failed to initialize subsystem " + subSystem.Name + $" (class {subSystem.GetType().Name})");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            log.Fatal(ex, "App is unable to continue running!");
            exception = ex;
            return false;
        }

        Toasts.Info("Subsystems Initialized");
        //new Dialog("test", "\\L[https://www.youtube.com/watch?v=xlBUy87c6y4|my awesome link]").Show();
        exception = null;
        return true;
    }

    internal void Tick()
    {
        foreach (SubSystem subSystem in subSystems)
        {
            subSystem.Update();
        }
    }

    internal void Draw()
    {

    }

    internal void Close()
    {

    }
}
