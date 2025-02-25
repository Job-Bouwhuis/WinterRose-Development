using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WinterRose.Monogame;

/// <summary>
/// Provides static properties to access the current game time. 
/// </summary>
public static class Time
{
    internal static GameTime time;
    static Stopwatch SinceSceneLoadTimer = new();

    static Time()
    {
        Setup();
    }

    internal static void Setup()
    {
        Worlds.Universe.OnNewWorldLoaded += () =>
        {
            if (SinceSceneLoadTimer.IsRunning)
                SinceSceneLoadTimer.Restart();
            else
                SinceSceneLoadTimer.Start();
        };
    }

    /// <summary>
    /// The time since the last frame in seconds
    /// </summary>
    public static float SinceLastFrame =>(float)time.ElapsedGameTime.TotalSeconds;
    /// <summary>
    /// The time since application startup in seconds
    /// </summary>
    public static float SinceStartup => (float)time.TotalGameTime.TotalSeconds;

    /// <summary>
    /// Whether the Update loop is taking longer than the TargetElapsedTime, this will cause the game to skip draw calls to try to catch up.
    /// </summary>
    public static bool IsRunningSlowly => time.IsRunningSlowly;

    /// <summary>
    /// The time since the last time a scene was loaded in seconds
    /// </summary>
    public static float SinceWorldLoad => (float)SinceSceneLoadTimer.Elapsed.TotalSeconds;

    internal static void Update(GameTime gameTime)
    {
        time = gameTime;
    }
}
