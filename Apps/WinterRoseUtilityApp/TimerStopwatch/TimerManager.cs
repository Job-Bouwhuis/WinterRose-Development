
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.TimerStopwatch;

internal static class TimerManager
{
    private const int MilestoneWindow = 2;
    private static List<RunningTimer> timers = new List<RunningTimer>();

    public static IReadOnlyList<RunningTimer> Timers => timers;

    internal static void Add(RunningTimer timer)
    {
        timers.Add(timer);
        timer.Start();
    }

    public static void Update()
    {
        for (int i = 0; i < timers.Count; i++)
        {
            RunningTimer timer = timers[i];
            timer.Update(TimeSpan.FromSeconds(Time.deltaTime));

            if (timer.IsCompleted)
            {
                timers.Remove(timer);
                i--;
                continue;
            }

            TimeSpan remaining = timer.Remaining;
            double totalSeconds = timer.Duration.TotalSeconds;

            List<int> milestoneThresholds = new List<int>();

            // generate milestone thresholds
            if (totalSeconds > 10) milestoneThresholds.Add(10);
            if (totalSeconds > 60) milestoneThresholds.Add(60);
            if (totalSeconds > 300) milestoneThresholds.Add(300);
            if (totalSeconds > 600)
            {
                int max10 = (int)Math.Min(totalSeconds / 60, 30);
                for (int m = 10; m <= max10; m += 10) milestoneThresholds.Add(m * 60);
            }
            if (totalSeconds > 1800)
            {
                int max30 = (int)Math.Min(totalSeconds / 60, 60);
                for (int m = 30; m <= max30; m += 30) milestoneThresholds.Add(m * 60);
            }
            if (totalSeconds > 3600)
            {
                int max60 = (int)(totalSeconds / 60);
                for (int m = 60; m <= max60; m += 60) milestoneThresholds.Add(m * 60);
            }

            // --- ENTRY MILESTONE ---
            if (!timer.HasNotifiedEntry)
            {
                bool inMilestoneWindow = milestoneThresholds.Any(threshold =>
                    Math.Abs(remaining.TotalSeconds - threshold) <= MilestoneWindow);

                if (!inMilestoneWindow)
                {
                    timer.HasNotifiedEntry = true;
                    Toast entryToast = ContainerCreators.TimerThresholdNotifier(timer, 8);
                    entryToast.Show();
                }
            }

            // now check other milestone thresholds
            foreach (int threshold in milestoneThresholds)
            {
                double diff = Math.Abs(remaining.TotalSeconds - threshold);
                if (diff <= MilestoneWindow && !timer.HasNotified(threshold))
                {
                    timer.MarkNotified(threshold);
                    if (threshold == 10)
                    {
                        Toast t = ContainerCreators.TimerThresholdNotifier(timer);
                        t.Style.PauseAutoDismissTimer = true;
                        t.Show();
                    }
                    else
                    {
                        ContainerCreators.TimerThresholdNotifier(timer).Show();
                    }
                    timer.HasNotifiedEntry = true;
                    break;
                }
            }
        }
    }
}