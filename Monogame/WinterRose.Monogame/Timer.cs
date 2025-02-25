using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

public class Timer
{
    /// <summary>
    /// The name of the timer (Can be used to identify the timer, and also to share timers between components)
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The interval in seconds between each tick
    /// </summary>
    public float Duration { get; set; }
    /// <summary>
    /// Wether the timer is running or not
    /// </summary>
    public bool IsRunning { get; private set; } = false;
    /// <summary>
    /// Wether the timer has finished or not
    /// </summary>
    public bool IsFinished { get; private set; } = false;
    /// <summary>
    /// Wether the timer is paused or not
    /// </summary>
    public bool IsPaused { get; private set; } = false;
    /// <summary>
    /// Wether the timer is looping or not
    /// </summary>
    public bool IsLooping { get; set; } = false;

    private Action<Timer> onTick = delegate { };

    private float currentTime = 0;

    /// <summary>
    /// Pauses the timer
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
        IsRunning = false;
    }

    /// <summary>
    /// Starts or resumes the timer
    /// </summary>
    public void Start()
    {
        IsPaused = false;
        IsRunning = true;
        IsFinished = false;
    }


    /// <summary>
    /// Starts a new timer, Or returns an existing timer with the same name. If an existing timer is found, and it is not running, it will be restarted.
    /// </summary>
    /// <param name="name">The identifier for this timer</param>
    /// <param name="duration">The duration, in seconds, between each tick of this timer</param>
    /// <param name="onTick">The action that is invoked each tick</param>
    /// <param name="isLooping">Whether this timer restarts itself after a tick has occured</param>
    /// <returns></returns>
    public static Timer StartNew(string name, float duration, Action<Timer> onTick, bool isLooping = false)
    {
        if (!timers.TryGetValue(name, out Timer? timer))
        {
            timer = new Timer()
            {
                Name = name,
                Duration = duration,
                IsLooping = isLooping
            };
            timer.onTick += onTick;

            timers.Add(name, timer);
        }

        timer.Duration = duration;
        timer.IsRunning = true;
        timer.IsFinished = false;
        timer.IsPaused = false;
        timer.IsLooping = isLooping;

        return timer;
    }

    /// <summary>
    /// Starts a new timer, This timer will have a unique name, and unless you store the name speperately, or pass the reference manually, you can not share this timer between components.<br></br>
    /// This timer is not stored among all timers when it is finished. 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="onTick"></param>
    /// <param name="isLooping"></param>
    /// <returns></returns>
    public static Timer StartNew(float duration, Action<Timer> onTick, bool isLooping = false)
    {
        return StartNew(Guid.NewGuid().ToString(), duration, onTick, isLooping);
    }

    /// <summary>
    /// Creates a new timer 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="duration"></param>
    /// <param name="onTick"></param>
    /// <param name="isLooping"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Timer CreateNew(string name, float duration, Action<Timer> onTick, bool isLooping = false)
    {
        if (timers.TryGetValue(name, out Timer? timer))
        {
            if (timer.Duration != duration)
                throw new Exception("Timer duration mismatch");
            return timer;
        }

        timer = new Timer()
        {
            Name = name,
            Duration = duration,
            onTick = onTick,
            IsLooping = isLooping
        };

        timers.Add(name, timer);

        return timer;
    }

    #region Internal timer management

    internal static Dictionary<string, Timer> timers = [];

    internal static void UpdateAll()
    {
        foreach (var timer in timers.Values)
        {
            if (timer.IsRunning && !timer.IsPaused)
            {
                timer.currentTime += Time.SinceLastFrame;

                if (timer.currentTime >= timer.Duration)
                {
                    try
                    {
                        timer.onTick?.Invoke(timer);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    
                    timer.currentTime = 0;

                    if (!timer.IsLooping)
                    {
                        timer.IsFinished = true;
                        timer.IsRunning = false;

                        if (Guid.TryParse(timer.Name, out _))
                            timers.Remove(timer.Name);
                    }
                }
            }
        }
    }

    internal static void ClearAll()
    {
        timers.Clear();
    }

    #endregion
}
