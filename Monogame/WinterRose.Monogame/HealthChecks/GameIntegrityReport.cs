using System;

namespace WinterRose.Monogame.HealthChecks;

/// <summary>
/// A report of the integrity of a game service
/// </summary>
/// <remarks>
/// Creates a new <see cref="GameIntegrityReport"/> with the given values.
/// </remarks>
/// <param name="checkName">The human friendly name given to the check that was evaluated</param>
/// <param name="status">The status the check returned</param>
/// <param name="exception">The exception that was thrown. may be null</param>
/// <param name="check">The check that was evaluated</param>
public class GameIntegrityReport(HealthCheck check, string checkName, HealthStatus status, Exception? exception = null)
{
    ///<summary>
    /// The human friendly name given to the check that was evaluated
    /// </summary>
    public string CheckName { get; } = checkName;
    /// <summary>
    /// The status of the check.
    /// </summary>
    public HealthStatus Status { get; } = status;
    /// <summary>
    /// The exception that was thrown when checking the integrity of the game. may be null if no exception was thrown.
    /// </summary>
    public Exception? Exception { get; } = exception;
    /// <summary>
    /// The check that was evaluated.
    /// </summary>
    public HealthCheck Check { get; } = check;
}
