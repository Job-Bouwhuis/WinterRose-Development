using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.HealthChecks;

/// <summary>
/// Keeps track of all the <see cref="HealthCheck"/>s and provides a way to check the integrity of the game.
/// </summary>
public static class HealthStatusManager
{
    /// <summary>
    /// Instructs the manager to run all the >see cref="HealthCheck"/>s every X frames.
    /// </summary>
    public static int CheckEveryXFrames { get; set; } = 60;
    private static int currentFrames = 0;
    public static GameIntegrityReportCollection LastChecks => cache;
    private static GameIntegrityReportCollection cache;

    private static List<HealthCheck> healthChecks = new();

    static HealthStatusManager()
    {
        foreach (var type in TypeWorker.FindTypesWithBase<HealthCheck>())
        {
            try
            {
                healthChecks.Add((ActivatorExtra.CreateInstance(type) as HealthCheck)!);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create instance of {type.FullName} with error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Instructs the manager to run all the >see cref="HealthCheck"/>s at the next frame.
    /// </summary>
    public static void RequestCheck() => currentFrames = CheckEveryXFrames;

    /// <summary>
    /// Runs all the <see cref="HealthCheck"/>s and returns a collection of <see cref="GameIntegrityReport"/>s
    /// </summary>
    /// <returns></returns>
    internal static GameIntegrityReportCollection CheckGameIntegrity()
    {
        if(currentFrames++ < CheckEveryXFrames && cache is not null)
            return cache;
        currentFrames = 0;
        List<GameIntegrityReport> checks = [];

        for (int i = 0; i < healthChecks.Count; i++)
        {
            HealthCheck? check = healthChecks[i];
            if (check is null)
            {
                healthChecks.RemoveAt(i);
                i--;
                continue;
            }
            try
            {
                checks.Add(new(check, check.humanFriendlyName, check.Check(), null));
            }
            catch (Exception e)
            {
                checks.Add(new(check, check.humanFriendlyName, HealthStatus.Catastrophic, e));
            }
        }
        return cache = checks;
    }

    /// <summary>
    /// Returns true when all the checks are healthy
    /// </summary>
    /// <param name="reports"></param>
    /// <returns></returns>
    internal static bool CheckGameIntegrity(out GameIntegrityReportCollection reports)
    {
        reports = CheckGameIntegrity();
        return reports.AllHealthy;
    }
}