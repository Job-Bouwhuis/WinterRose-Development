namespace WinterRose.Monogame.HealthChecks;

/// <summary>
/// A status that represents the health of a service.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The status is unknown.
    /// </summary>
    Unknown,
    /// <summary>
    /// The service is healthy.
    /// </summary>
    Healthy,
    /// <summary>
    /// The service is damaged but still (partly) functional.
    /// </summary>
    Damaged,
    /// <summary>
    /// The service is unhealthy and not functional.
    /// </summary>
    Unhealthy,
    /// <summary>
    /// An error occurred while checking the service.
    /// </summary>
    Catastrophic
}
