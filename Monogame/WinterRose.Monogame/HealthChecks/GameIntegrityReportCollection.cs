using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterRose.Monogame.HealthChecks;

/// <summary>
/// A collection of <see cref="GameIntegrityReport"/>s
/// </summary>
public class GameIntegrityReportCollection
{
    /// <summary>
    /// Makes a new <see cref="GameIntegrityReportCollection"/> with the given reports.
    /// </summary>
    /// <param name="reports"></param>
    public GameIntegrityReportCollection(List<GameIntegrityReport> reports)
    {
        AllEvaluatedChecks = reports;
    }

    /// <summary>
    /// All the checks that were evaluated.
    /// </summary>
    public List<GameIntegrityReport> AllEvaluatedChecks { get; } = [];
    /// <summary>
    /// Whether or not all the checks were healthy.
    /// </summary>
    public bool AllHealthy => AllEvaluatedChecks.All(x => x.Status == HealthStatus.Healthy);
    /// <summary>
    /// Whether or not at least one check encountered a catastrophic failure.
    /// </summary>
    public bool CatastrophicFailure => AllEvaluatedChecks.Any(x => x.Status == HealthStatus.Catastrophic);

    /// <summary>
    /// The amount of checks that were damaged.
    /// </summary>
    public int DamagedChecks => AllEvaluatedChecks.Count(x => x.Status == HealthStatus.Damaged);
    /// <summary>
    /// The amount of checks that were unhealthy.
    /// </summary>
    public int UnhealthyChecks => AllEvaluatedChecks.Count(x => x.Status == HealthStatus.Unhealthy);
    /// <summary>
    /// The amount of checks that were unknown.
    /// </summary>
    public int UnknownChecks => AllEvaluatedChecks.Count(x => x.Status == HealthStatus.Unknown);
    /// <summary>
    /// The amount of checks that were healthy.
    /// </summary>
    public int HealthyChecks => AllEvaluatedChecks.Count(x => x.Status == HealthStatus.Healthy);
    /// <summary>
    /// The amount of checks that encountered a catastrophic failure.
    /// </summary>
    public int CatastrophicChecks => AllEvaluatedChecks.Count(x => x.Status == HealthStatus.Catastrophic);
    /// <summary>
    /// The total amount of checks that were evaluated.
    /// </summary>
    public int TotalChecks => AllEvaluatedChecks.Count;
    /// <summary>
    /// The total amount of checks that are not healthy.
    /// </summary>
    public int TotalFailedChecks => DamagedChecks + UnhealthyChecks + UnknownChecks + CatastrophicChecks;

    /// <summary>
    /// Gets the enumerator for the <see cref="AllEvaluatedChecks"/> dictionary.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<GameIntegrityReport> GetEnumerator()
    {
        return AllEvaluatedChecks.GetEnumerator();
    }

    public static implicit operator GameIntegrityReportCollection(List<GameIntegrityReport> reports)
    {
        return new(reports);
    }
}
