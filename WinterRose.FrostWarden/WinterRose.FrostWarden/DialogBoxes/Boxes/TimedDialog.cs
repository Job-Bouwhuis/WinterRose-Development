using Raylib_cs;
using System.Threading;
using WinterRose.FrostWarden.TextRendering;

namespace WinterRose.FrostWarden.DialogBoxes.Boxes
{
    public class TimedDialog : Dialog
    {
        private readonly Action? onTimeout;

        public float TimeRemaining { get; private set; }
        public float TotalTime { get; }

        public TimedDialog(
             string title,
             string message,
             float totalTime,
             Action? onTimeout = null,
             DialogPlacement placement = DialogPlacement.CenterSmall,
             DialogPriority priority = DialogPriority.Normal,
             string[]? buttons = null,
             Action[]? onButtonClick = null,
             Action<UIContext>? onImGui = null)
             : base(title, message, placement, priority, buttons ?? [], onButtonClick ?? [], onImGui)
        {
            TotalTime = totalTime;
            this.onTimeout = onTimeout;
        }

        public override void Update()
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                IsClosing = true;
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