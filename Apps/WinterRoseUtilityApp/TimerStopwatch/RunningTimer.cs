using Microsoft.Graph.Communications.OnlineMeetings.GetAllTranscriptsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

namespace WinterRoseUtilityApp.TimerStopwatch;
internal class RunningTimer
{
    public string Name;
    public TimeSpan Duration;
    public TimeSpan Elapsed;
    public DateTime StartTime;
    public bool IsRunning;
    public bool IsCompleted;
    public bool AutoRemoveOnFinish;
    public bool Repeat;
    public MulticastVoidInvocation<RunningTimer> OnFinished { get; } = new();

    private HashSet<int> notifiedMilestones = new HashSet<int>();
    public TimeSpan Remaining => Duration - Elapsed;

    public bool HasNotifiedEntry { get; internal set; }

    public void Update(TimeSpan delta)
    {
        if (!IsRunning || IsCompleted)
            return;

        Elapsed += delta;

        if (Elapsed >= Duration)
        {
            Elapsed = Duration;
            IsCompleted = true;
            IsRunning = false;
            OnFinished?.Invoke(this);
            if (Repeat)
            {
                Reset();
                Start();
            }
        }
    }

    public void Start()
    {
        IsRunning = true;
        IsCompleted = false;
    }
    public void Reset(bool andRestart = true)
    {
        Elapsed = TimeSpan.Zero;
        if (andRestart)
            Start();
    }

    
    public bool HasNotified(int seconds) => notifiedMilestones.Contains(seconds);
    public void MarkNotified(int seconds) => notifiedMilestones.Add(seconds);
}
