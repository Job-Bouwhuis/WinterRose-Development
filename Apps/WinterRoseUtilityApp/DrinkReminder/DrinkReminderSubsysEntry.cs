using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose;
using WinterRose.EventBusses;
using WinterRose.ForgeThread;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.WinterForgeSerializing;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.DrinkReminder;

internal class DrinkReminderSubsysEntry : SubSystem
{
    private const string INTERVAL_ASSET_NAME = "DrinkInterval";

    DrinkSettings settings;

    Sound reminderSound;
    int time;
    private UICircleProgress? currentTimerProgressBar;

    public DrinkReminderSubsysEntry()
        : base("Drink Reminder", "Reminds one to drink their fluids", new Version(1, 0, 0))
    {
    }

    public override void Init()
    {
        Program.Current.AddTrayItem(new UIButton("Drink Reminder", (c, b) => CreateWindow().Show()));

        if(!Assets.Exists(INTERVAL_ASSET_NAME))
        {
            settings = DrinkSettings.Default;
            Assets.CreateAsset(settings, INTERVAL_ASSET_NAME);
            return;
        }

        settings = Assets.Load<DrinkSettings>(INTERVAL_ASSET_NAME);
        reminderSound = Assets.Load<Sound>("subnautica");
    }

    public override void Update()
    {
        time += (Time.deltaTime * 1000).FloorToInt();
        var remaining = TimeSpan.FromMinutes(settings.ReminderIntervalMinutes) - TimeSpan.FromMilliseconds(time);
        if (currentTimerProgressBar is not null)
        {
            currentTimerProgressBar.Text = remaining.Hours > 0
                ? string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", remaining.Hours, remaining.Minutes, remaining.Seconds, remaining.Milliseconds / 100)
                : string.Format("{0:D2}:{1:D2}.{2}", remaining.Minutes, remaining.Seconds, remaining.Milliseconds / 100);

            double totalIntervalMs = TimeSpan.FromMinutes(settings.ReminderIntervalMinutes).TotalMilliseconds;
            double elapsedMs = totalIntervalMs - remaining.TotalMilliseconds;
            currentTimerProgressBar.SetProgress((float)Math.Clamp(elapsedMs / totalIntervalMs, 0, 1));
        }

        if (time > settings.ReminderIntervalMilliseconds)
        {
            time = 0;
            Remind();
        }
    }

    private void Remind()
    {
        int index = Random.Shared.Next(0, settings.Messages.Count);
        Toasts.Error(settings.Messages[index], ToastRegion.Center);
        Raylib.PlaySound(reminderSound);
    }

    private UIWindow CreateWindow()
    {
        UIWindow window = new UIWindow("Drink Reminder", 300, 500);
        window.OnClosing.Subscribe((w) => currentTimerProgressBar = null);

        UIText label = new("Every change to the slider is saved immediately", UIFontSizePreset.Text);
        window.AddContent(label);
        UIValueSlider<int> intervalSlider = new UIValueSlider<int>
        {
            Label = "Reminder Interval (minutes)",
            MinValue = 15,
            MaxValue = 180,
            Step = 15,
            Value = 60,
            SnapToStep = true
        };
        window.AddContent(intervalSlider);
        intervalSlider.OnValueChangedBasic.Subscribe(Invocation.Create<int>(NewValueSelected));
        intervalSlider.SetValue(settings.ReminderIntervalMinutes, false);

        currentTimerProgressBar = new UICircleProgress()
        {
            DontShowProgressPercent = true,
            AlwaysShowText = true
        };
        window.AddContent(currentTimerProgressBar);

        window.AddButton("Simulate reminder", Invocation.Create((IUIContainer c, UIButton b) => Remind()));
        return window;
    }

    private void NewValueSelected(int newValue)
    {
        settings.ReminderIntervalMinutes = newValue;
        Assets.Save(INTERVAL_ASSET_NAME, settings);
    }
}
