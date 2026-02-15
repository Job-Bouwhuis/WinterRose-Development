using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeThread;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
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

    public bool Initialized { get; private set; } = false;

    public CoroutineHandle Initialize()
    {
        return ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn("Main", initializeCoroutine());
    }

    private IEnumerator initializeCoroutine()
    {
        Toast progressToast = new Toast(ToastType.Info, ToastRegion.Center, ToastStackSide.Top);
        progressToast.Style.TimeUntilAutoDismiss = 0;
        UIText text = new UIText("Loading subsystems... \\e[]", UIFontSizePreset.Title);
        progressToast.AddContent(text);
        UIText currentManagerText = new UIText("\\e[] Setting up...");
        progressToast.AddContent(currentManagerText);
        progressToast.Show();
        progressToast.ContinueWith(new Toast(ToastType.Success, ToastRegion.Center, ToastStackSide.Top)
            .AddText("Subsystem initialization finished", UIFontSizePreset.Subtitle));

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
            currentManagerText.Text = $"Creating subsystem '{t.Name}'";
            yield return TimeSpan.FromMilliseconds(10);
            var stopwatch = Stopwatch.StartNew();
            SubSystem subsys;
            try
            {
                subsys = (SubSystem)DynamicObjectCreator.CreateInstance(t, []);
            }
            catch (Exception ex)
            {
                log.Critical(ex, "Failed to start subsystem " + t.Name);
                Toasts.Error($"Creating subsystem '{t.Name}' failed!");
                continue;
            }
            stopwatch.Stop();

            log.Info($"Took {stopwatch.Elapsed.TotalMilliseconds}ms");
            if (subsys is null)
            {
                log.Error($"Failed to start subsystem {t.Name}");
                Toasts.Error($"Creating subsystem '{t.Name}' failed!");
                continue;
            }
            subSystems.Add(subsys);
            yield return TimeSpan.FromMilliseconds(10);
        }

        foreach (SubSystem subSystem in subSystems)
        {
            currentManagerText.Text = $"Initializing subsystem '{subSystem.Name}'";
            yield return TimeSpan.FromMilliseconds(10);
            try
            {
                subSystem.Init();
            }
            catch (Exception ex)
            {
                log.Critical(ex, "Failed to initialize subsystem " + subSystem.Name + $" (class {subSystem.GetType().Name})");
                Toasts.Error($"Starting subsystem '{subSystem.Name}' failed!");
                continue;
            }
            yield return TimeSpan.FromMilliseconds(10);
        }

        Initialized = true;
        progressToast.Close();
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
