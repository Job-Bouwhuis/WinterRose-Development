using BulletSharp.SoftBody;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.Helldivers2;
using WinterRoseUtilityApp.Helldivers.IconFetcher;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.Helldivers;

// api info: https://helldivers-2.github.io/api/#/
/*
    HEALTH property in the API stands for completion where 0% is completed
 
 */

[SubSystems.SubSystemSkip]
internal class HelldiversEntry : SubSystem
{
    private Helldivers2 HelldiversClient;
    private HelldiversWarMonitor WarMonitor;
    private Assignment? CurrentMajorOrder;
    private War? CurrentWarState;
    private readonly RateLimitManager rateLimitManager;

    public HelldiversEntry() : base("Helldivers", "Get all the information about the current situation in the Helldivers universe", new Version(1, 0, 0))
    {
        System.Threading.Tasks.Task.Run(HelldiversIconCollection.RegisterAllAsRichIcon).ConfigureAwait(false);
        rateLimitManager = new RateLimitManager(log);
    }

    public override void Init()
    {
        HelldiversClient = new Helldivers2();
        WarMonitor = new HelldiversWarMonitor(HelldiversClient, rateLimitManager, log);

        // Subscribe to war monitor events
        WarMonitor.WarStateUpdated += OnWarStateUpdated;
        WarMonitor.PlanetLiberated += OnPlanetLiberated;
        WarMonitor.PlanetLiberationMilestone += OnPlanetLiberationMilestone;
        WarMonitor.SuperEarthMessage += OnSuperEarthMessage;

        // Start monitoring with 30 second interval
        WarMonitor.Start(updateIntervalSeconds: 30);

        System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
        {
            FetchInitialData();
        });

        Program.Current.AddTrayItem(
                new UIButton("Helldivers War Status", (c, b) => ShowWarStatusWindow())
            );
        Program.Current.AddTrayItem(
            new UIButton("Helldivers Newsfeed", (c, b) => ShowNewsfeedWindow())
        );


        log.Info("Helldivers subsystem initialized");
    }

    private void ShowNewsfeedWindow()
    {
        UIWindow window = new UIWindow("Newsfeed", 500, 800);

        window.AddText("Fetching.... \\e[]");

        DateTime cutoffDate = DateTime.UtcNow.AddDays(-30);

        rateLimitManager.ExecuteAsync(
            HelldiversClient.GetApiV2DispatchesAllAsync,
            "GetNewsfeed")
            .ContinueWith(newsTask =>
            {
                window.ClearContent();
                if(newsTask.IsCanceled)
                {
                    window.AddText("\\c[red]\\tw[]Task unexpectedly interupted!");
                    return;
                }
                if (newsTask.IsFaulted || newsTask.Result is null)
                {
                    if(newsTask.Result is null)
                    {
                        window.AddText("\\c[red]\\tw[]Fetch did not result in newsfeed items");
                    }
                    if (newsTask.Exception is Exception e)
                    {
                        List<Exception> exs = [];
                        if (e is AggregateException aggr)
                            exs.AddRange(aggr.InnerExceptions);
                        else
                            exs.Add(e);

                        foreach(var ex in exs)
                        {
                            window.AddContent(new UIText(ex.GetType().Name, UIFontSizePreset.Subtitle));
                            window.AddContent(new UIText(ex.Message, UIFontSizePreset.Text));
                            window.AddContent(new UISpacer(8));
                        }

                    }
                }

                var recentMessages = newsTask.Result
                    .Where(m => m.Published >= cutoffDate)
                    .OrderByDescending(m => m.Published)
                    .ToList();

                foreach (var message in recentMessages)
                {
                    string header = $"[{message.Published:u}]";
                    string body = message.Message ?? string.Empty;

                    window.AddContent(new UIText(header, UIFontSizePreset.Subtitle));
                    window.AddContent(new UIText(body, UIFontSizePreset.Text));
                    window.AddContent(new UISpacer(8));
                }
            });

        window.Show();
    }


    private void FetchInitialData()
    {
        // War
        rateLimitManager.ExecuteAsync(
            async () => await HelldiversClient.GetApiV1WarAsync(),
            "GetApiV1War - Initial")
            .ContinueWith(warTask =>
            {
                if (warTask.IsFaulted)
                {
                    log.Error(warTask.Exception, "Failed to fetch war data");
                    return;
                }

                CurrentWarState = warTask.Result;
            });

        // Major orders
        rateLimitManager.ExecuteAsync(
            async () => await HelldiversClient.GetApiV1AssignmentsAllAsync(),
            "GetApiV1AssignmentsAll - Initial")
            .ContinueWith(assignmentsTask =>
            {
                if (!assignmentsTask.IsFaulted &&
                    assignmentsTask.Result is not null &&
                    assignmentsTask.Result.Count > 0)
                {
                    var newOrder = assignmentsTask.Result.FirstOrDefault();
                    if (newOrder is not null)
                    {
                        NotifyMajorOrderIfChanged(newOrder);
                        CurrentMajorOrder = newOrder;
                    }
                }
            });
    }

    private void OnWarStateUpdated(War warData)
    {
        CurrentWarState = warData;

        HelldiversClient.GetApiV1AssignmentsAllAsync().ContinueWith(assignmentsTask =>
        {
            if (!assignmentsTask.IsFaulted)
            {
                var newOrder = assignmentsTask.Result.FirstOrDefault();
                if (newOrder is not null)
                {
                    NotifyMajorOrderIfChanged(newOrder);
                    CurrentMajorOrder = newOrder;
                }
            }
            log.Warning(assignmentsTask.Exception, "Failed to fetch assignment!");
        });
    }

    public void NotifyMajorOrderIfChanged(Assignment newMajorOrder)
    {
        if (ShouldNotifyMajorOrder(newMajorOrder))
        {
            OnMajorOrderUpdated(newMajorOrder);
        }
    }

    private bool ShouldNotifyMajorOrder(Assignment newMajorOrder)
    {
        if (CurrentMajorOrder is null)
            return true; // No current major order, so show it
        // Show if the new major order is different from the current one
        return newMajorOrder.Id != CurrentMajorOrder.Id;
    }

    private void OnPlanetLiberated(Planet planet)
    {
        string planetName = planet.Name?.ToString() ?? "Unknown Planet";
        var toast = new Toast(ToastType.Success, ToastRegion.Center, ToastStackSide.Top)
            .AddText($"WE WON! {planetName} has been liberated!", UIFontSizePreset.Title);
        toast.Style.TimeUntilAutoDismiss = 10;
        Toasts.ShowToast(toast);
    }

    private void OnPlanetLiberationMilestone(Planet planet, float liberation)
    {
        Toasts.Info($"{planet.Name}: {(int)liberation}% liberated!", ToastRegion.Left, ToastStackSide.Bottom);
    }

    private void OnSuperEarthMessage(Dispatch2 message)
    {
        HelldiversUIHelper.ShowSuperEarthMessageToast(message);
    }

    private void OnMajorOrderUpdated(Assignment assignment)
    {
        CurrentMajorOrder = assignment;
        log.Info($"Major Order updated: {assignment.Title}");
        HelldiversUIHelper.ShowMajorOrderToast(assignment);
    }

    private void ShowWarStatusWindow()
    {
        HelldiversUIHelper.ShowMajorOrderToast(CurrentMajorOrder);
    }

    public override void Destroy()
    {
        WarMonitor?.Stop();
        log.Info("Helldivers subsystem destroyed");
    }
}
