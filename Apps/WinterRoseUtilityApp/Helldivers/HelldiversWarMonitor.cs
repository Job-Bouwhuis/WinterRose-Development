using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using WinterRose.Helldivers2;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.Helldivers;

/// <summary>
/// Monitors the Helldivers war status and notifies about updates.
/// Fetches campaign data separately and tracks liberation changes.
/// </summary>
internal class HelldiversWarMonitor
{
    private readonly Helldivers2 helldiversClient;
    private readonly Log log;
    private readonly MessageTracker messageTracker;
    private readonly RateLimitManager rateLimitManager;
    
    private War? lastWarState;
    private Dictionary<int, float> liberationPercentageSnapshots = [];
    
    private System.Threading.Timer? monitoringTimer;
    private bool firstLoad = true;

    public List<Planet> Planets { get; private set; } = [];

    public event Action<War>? WarStateUpdated;
    public event Action<Planet, float>? PlanetLiberationMilestone; // 10% increments
    public event Action<Planet>? PlanetLiberated;
    public event Action<Dispatch2>? SuperEarthMessage;

    public HelldiversWarMonitor(Helldivers2 helldiversClient, RateLimitManager rateLimiter, Log log)
    {
        this.helldiversClient = helldiversClient;
        this.log = log;
        messageTracker = new MessageTracker(log);
        rateLimitManager = rateLimiter;
    }

    public void Start(int updateIntervalSeconds = 30)
    {
        if (monitoringTimer is not null)
        {
            log.Info("War monitor is already running");
            return;
        }

        int intervalMs = updateIntervalSeconds * 1000;
        monitoringTimer = new System.Threading.Timer(_ => UpdateWar(), null, 0, intervalMs);
        log.Info($"War monitor started with {updateIntervalSeconds}s interval");
    }

    public void Stop()
    {
        if (monitoringTimer is null)
        {
            log.Info("War monitor is not running");
            return;
        }

        monitoringTimer.Dispose();
        monitoringTimer = null;
        log.Info("War monitor stopped");
    }

    public War? GetLastWarState() => lastWarState;

    private void UpdateWar()
    {
        try
        {
            // Fetch war state from the v1 API
            rateLimitManager.ExecuteAsync(
                async () => await helldiversClient.GetApiV1WarAsync(),
                "GetApiV1War"
            ).ContinueWith(warTask =>
            {
                if (warTask.IsFaulted)
                {
                    log.Error(warTask.Exception, "Failed to fetch war state");
                    return;
                }

                var newWarState = warTask.Result;
                if (newWarState is null)
                {
                    log.Info("Received null war state from API");
                    return;
                }

                lastWarState = newWarState;
                WarStateUpdated?.Invoke(newWarState);
                
                // Fetch campaigns separately since War doesn't contain them
                ProcessCampaignsAsync();
                
                // Process news feed with proper rate limiting
                ProcessNewsFeedAsync(lastWarState);
            });
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error during war monitoring");
        }
    }

    /// <summary>
    /// Fetches all active campaigns and processes planet liberation data.
    /// Campaigns contain the Planet reference which has health/liberation data.
    /// </summary>
    private void ProcessCampaignsAsync()
    {
        try
        {
            rateLimitManager.ExecuteAsync(
                async () => await helldiversClient.GetApiV1CampaignsAllAsync(),
                "GetApiV1CampaignsAll"
            ).ContinueWith(campaignsTask =>
            {
                if (campaignsTask.IsFaulted)
                {
                    log.Warning("Campaign fetching rate limited. trying again later");
                    return;
                }

                var campaigns = campaignsTask.Result;
                if (campaigns is null || campaigns.Count == 0)
                {
                    log.Debug("No active campaigns found");
                    return;
                }

                foreach (var campaign in campaigns)
                {
                    // Campaign2 directly has a Planet property - no reflection needed
                    if (campaign?.Planet is null)
                        continue;

                    if(!Planets.Any(p => p.Index == campaign.Planet.Index))
                        Planets.Add(campaign.Planet);

                    ProcessPlanetLiberation(campaign.Planet);
                }
                firstLoad = false;
            });
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error processing campaigns");
        }
    }

    private void ProcessPlanetLiberation(Planet planet)
    {
        if (planet.Disabled)
            return; // disabled planets don't change liberation status and shouldn't trigger milestones

        int planetIndex = planet.Index;
        float newLiberation = GetLiberationPercentage(planet);
        bool isLiberated = newLiberation >= 99.9f;
        if (newLiberation != 0)
        {

        }
        // Check for liberation milestone
        if (!liberationPercentageSnapshots.TryGetValue(planetIndex, out float lastSnapshot))
        {
            lastSnapshot = 0;
        }

        int currentPercent = (int)newLiberation;
        int lastPercent = (int)lastSnapshot;

        // Trigger on 10% milestones
        if (currentPercent >= 1 && ((currentPercent % 10 == 0 && currentPercent > lastPercent) || firstLoad))
        {
            string planetName = planet.Name?.ToString() ?? $"Planet {planetIndex}";
            log.Info($"Planet {planetName} ({planetIndex}) liberation: {currentPercent}%");
            PlanetLiberationMilestone?.Invoke(planet, newLiberation);
        }

        // Trigger on liberation
        if (isLiberated && lastSnapshot < 99.9f)
        {
            string planetName = planet.Name?.ToString() ?? $"Planet {planetIndex}";
            log.Info($"Planet {planetName} ({planetIndex}) has been liberated!");
            PlanetLiberated?.Invoke(planet);
        }

        liberationPercentageSnapshots[planetIndex] = newLiberation;
    }

    private void ProcessNewsFeedAsync(War war)
    {
        try
        {
            rateLimitManager.ExecuteAsync(
                helldiversClient.GetApiV2DispatchesAllAsync,
                "GetRawApiNewsFeed801"
            ).ContinueWith(feedTask =>
            {
                if (feedTask.IsFaulted)
                {
                    var ex = feedTask.Exception?.InnerException ?? feedTask.Exception;
                    
                    // Handle rate limiting gracefully
                    if (ex?.GetType().Name == "ApiException" && ex.GetType().GetProperty("StatusCode")?.GetValue(ex) is 429)
                    {
                        log.Info("News feed rate limited (429) - will retry later");
                        return;
                    }
                    
                    log.Error(ex, "Failed to fetch news feed");
                    return;
                }

                var newsFeed = (List<Dispatch2>)feedTask.Result;
                if (newsFeed is null || newsFeed.Count == 0)
                    return;

                for (int i = 0; i < newsFeed.Count; i++)
                {
                    var message = newsFeed[i];
                    if (message?.Message is null)
                        continue;

                    if (!messageTracker.ShouldShowNewsFeedItem(message.Id, message.Published))
                    {
                        continue;
                    }

                    // Check if it's a Super Earth message
                    if (IsSuperEarthMessage(message))
                    {
                        log.Info($"Super Earth message detected: {message.Message}");
                        SuperEarthMessage?.Invoke(message);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error processing news feed");
        }
    }

    private bool IsSuperEarthMessage(Dispatch2 message)
    {
        if (message?.Message is null)
            return false;

        string lowerMsg = message.Message.ToLowerInvariant();
        return lowerMsg.Contains("super earth") || lowerMsg.Contains("high command");
    }

    private static float GetLiberationPercentage(Planet planet)
    {
        float h = planet.Health;
        float mh = planet.MaxHealth;
        return mh > 0 ? (1 - h / mh) : 0;
    }
}
