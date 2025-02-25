using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.HealthChecks;

/// <summary>
/// When implemented, provides a way to check the health of a service. things like world objects, specific components, ect.<br></br>
/// Use for things like if a database connection is still active, or if one or more world objects are still active that are required for the game to run.<br></br>
/// 
/// This is a base class that inherits from <see cref="Attribute"/> so that it can be used as such on a class should you implement a system yourself that requires these checks to be an attribute<br></br>
/// Inherit from it and implement the <see cref="Check"/> method to create a health check.
/// </summary>
/// <remarks>
/// You can use this to provide a human friendly name for your health checks. i advice to use a constant value when calling the base constructor for better usability.
/// </remarks>
/// <param name="humanFriendlyName"></param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public abstract class HealthCheck(string humanFriendlyName) : Attribute
{
    /// <summary>
    /// A message that will be used to describe the health of the service.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// A human friendly name for the health check.
    /// </summary>
    public required string humanFriendlyName = humanFriendlyName;
    /// <summary>
    /// When implemented, checks the health of a service.
    /// </summary>
    public abstract HealthStatus Check();
}
