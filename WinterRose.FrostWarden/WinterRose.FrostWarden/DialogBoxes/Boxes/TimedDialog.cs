using Raylib_cs;
using System.Threading;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes.Boxes
{
    public class TimedDialog : Dialog
    {
        private readonly Action? onTimeout;

        public float TimeRemaining => TotalTime - timeShown;
        public float TotalTime { get; }
        private float timeShown;
        public TimedDialog(
             string title,
             string message,
             float totalTime,
             Action? onTimeout = null,
             DialogPlacement placement = DialogPlacement.CenterSmall,
             DialogPriority priority = DialogPriority.Normal,
             string[]? buttons = null,
             Func<bool>[]? onButtonClick = null,
             Action<UIContext>? onImGui = null)
             : base(title, message, placement, priority, buttons ?? [], onButtonClick ?? [], onImGui)
        {
            TotalTime = totalTime;
            this.onTimeout = onTimeout;
        }

        public override void Update()
        {
            timeShown += Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                Close();
                onTimeout?.Invoke();
            }
        }

        public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            string timeLabel = $"Closing in {MathF.Round(TimeRemaining, 1)}s";
            ray.DrawText(timeLabel, (int)bounds.X + (int)padding, y, 16, Style.TimeLabelColor);
        }
    }
}