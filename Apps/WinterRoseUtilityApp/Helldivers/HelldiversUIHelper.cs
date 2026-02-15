using System;
using WinterRose.Helldivers2;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRoseUtilityApp.Helldivers;

/// <summary>
/// Helper class for showing Helldivers related UI notifications.
/// </summary>
internal class HelldiversUIHelper
{
    public static void ShowSuperEarthMessageToast(Dispatch2 message)
    {
        Toast t = new Toast(ToastType.Highlight, ToastRegion.Left, ToastStackSide.Top);
        t.AddContent(new HTMLContent(message.Message));
        t.Style.TimeUntilAutoDismiss = 10;
        t.Style.PauseAutoDismissTimer = false;
        t.Show();
    }

    public static void ShowMajorOrderToast(Assignment assignment)
    {
        if (assignment is null)
            return;

        string title = "Major Order";
        string briefing = "";

        if (!string.IsNullOrWhiteSpace(assignment.Title))
            title = assignment.Title;

        if (!string.IsNullOrWhiteSpace(assignment.Briefing))
            briefing = assignment.Briefing;

        var toast = new Toast(ToastType.Highlight, ToastRegion.Center, ToastStackSide.Top)
            .AddText(title, UIFontSizePreset.Title);

        if (!string.IsNullOrWhiteSpace(briefing))
        {
            string briefingPreview =
                briefing.Length > 150 ? briefing[..150] + "..." : briefing;

            toast.AddText(briefingPreview, UIFontSizePreset.Text);
        }

        toast.AddText("Rewards:" , UIFontSizePreset.Subtitle);
        toast.AddText($"* {assignment.Reward.Amount} {assignment.Reward.Name}");

        if (assignment.Tasks != null && assignment.Tasks.Count > 0)
        {
            toast.AddText("Objectives", UIFontSizePreset.Subtitle);

            int previewCount = 3;
            int shown = 0;

            foreach (var task in assignment.Tasks)
            {
                if (shown >= previewCount)
                    break;

                string text = task.ToString();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    toast.AddText($"* {text}", UIFontSizePreset.Text);
                    shown++;
                }

                float cur = task.Progress;
                float progress = task.Goal();

                float percentage = cur / progress;
                UIProgress prog = new UIProgress
                {
                    ProgressValue = percentage,
                    allowPauseAutoDismissTimer = false
                };
                toast.AddContent(prog);
            }

            if (assignment.Tasks.Count > previewCount)
            {
                int remaining = assignment.Tasks.Count - previewCount;
                toast.AddText($"+{remaining} more", UIFontSizePreset.Subtext);
            }
        }

        toast.Style.TimeUntilAutoDismiss = 15;
        Toasts.ShowToast(toast);
    }

}
