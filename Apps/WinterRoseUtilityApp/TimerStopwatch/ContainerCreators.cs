using Microsoft.Graph.Drives.Item.Items.Item.Workbook.CloseSession;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.TimerStopwatch;

internal class ContainerCreators
{
    internal static UIWindow AddTimer()
    {
        UIWindow window = new UIWindow("Add Timer", 500, 500);
        UIColumns cols = new();
        window.AddContent(cols);

        // timer name
        UITextInput nameInput = new();
        nameInput.Placeholder = "Timer name...";
        cols.AddToColumn(0, nameInput);

        UIColumns durationCols = new();

        // duration input (you could replace with dropdowns or number boxes later)
        UINumericUpDown<int> durationInput = new();

        durationCols.AddToColumn(0, durationInput);

        UIDropdown<string> timeUnit = new UIDropdown<string>();
        timeUnit.AddOption("Seconds");
        timeUnit.AddOption("Minutes");
        timeUnit.AddOption("Hours");
        durationCols.AddToColumn(1, timeUnit);
        timeUnit.SelectedIndex = 0;

        cols.AddToColumn(0, durationCols);
        // optional: repeat checkbox
        UICheckBox repeatCheck = new UICheckBox("Repeat timer");
        cols.AddToColumn(0, repeatCheck);

        // control buttons
        UIColumns controlCols = new();
        controlCols.AddToColumn(0, new UIButton("Cancel", (c, b) => c.Close()));
        controlCols.AddToColumn(1, new UIButton("Add Timer", (c, b) =>
        {
            string name = nameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Toasts.Error("Timer must have a name!");
                nameInput.Focus();
                return;
            }

            int time = durationInput.Value;
            if (time <= 0)
            {
                Toasts.Error("Please enter a valid duration (in seconds).");
                durationInput.Focus();
                return;
            }

            RunningTimer timer = new()
            {
                Name = name,
                Duration = timeUnit.SelectedItem switch
                {
                    "Seconds" => TimeSpan.FromSeconds(time),
                    "Minutes" => TimeSpan.FromMinutes(time),
                    "Hours" => TimeSpan.FromHours(time),
                    "Days" => TimeSpan.FromDays(time),
                    _ => throw new InvalidOperationException("Invalid time unit!")
                },
                Repeat = repeatCheck.Checked,
            };

            TimerManager.Add(timer);
            c.Close();
        }));

        cols.AddToColumn(0, controlCols);
        return window;
    }

    internal static Toast TimerThresholdNotifier(RunningTimer timer, float autoDismissTimer = 6)
    {
        Toast toast = new Toast(ToastType.Neutral, ToastRegion.Left, ToastStackSide.Top);
        toast.Style.TimeUntilAutoDismiss = autoDismissTimer;
        toast.Style.AutoScale = true;
        UICircleProgress progress = CreateProgressBarForTimer(timer, toast);
        progress.AlwaysShowText = true;
        progress.DontShowProgressPercent = true;
        toast.AddText("timer: " + timer.Name, UIFontSizePreset.Title);
        toast.AddContent(progress);

        return toast;
    }

    private static UICircleProgress CreateProgressBarForTimer(RunningTimer timer, UIContainer owner)
    {
        UICircleProgress progress = new(0, (self, cur) =>
        {
            var rem = timer.Remaining.TotalSeconds;
            var tot = timer.Duration.TotalSeconds;
            if (rem <= 10)
                self.Owner.Style.PauseAutoDismissTimer = true;

            if (timer.Remaining.Hours > 0)
                self.Text = $"{timer.Remaining:hh\\:mm\\:ss\\.f}";
            else
                self.Text = $"{timer.Remaining:mm\\:ss\\.f}";

            if (timer.IsCompleted)
            {
                if (owner is Toast)
                    owner.Style.PauseAutoDismissTimer = true;

                self.Text = $"\\c[#50FF50]Timer '{timer.Name}' completed!";
                self.ProgressProvider = null;
                self.Style.ProgressBarFill = new Raylib_cs.Color(0, 255, 0);

                if (owner is Toast)
                    owner.AddContent(new UIButton("Dismiss", (c, b) => c.Close()));
                return 1;
            }

            return (float)(rem / tot);
        });
        return progress;
    }

    internal static UIWindow ViewTimers()
    {
        UIWindow window = new UIWindow("Active Timers", 400, 600);
        Color ProgressBarDefault = window.Style.ProgressBarFill;

        if (TimerManager.Timers.Count == 0)
        {
            window.AddContent(new UIText("No active timers.", UIFontSizePreset.Title));
            return window;
        }

        foreach (var timer in TimerManager.Timers)
        {
            UITreeNode timerContainer = new UITreeNode(timer.Name);
            window.AddContent(timerContainer);
            UICircleProgress progress = CreateProgressBarForTimer(timer, window);
            window.Style.ProgressBarFill = ProgressBarDefault;
            progress.AlwaysShowText = true;
            progress.DontShowProgressPercent = true;
            timerContainer.AddChild(progress);
        }
        return window;
    }
}