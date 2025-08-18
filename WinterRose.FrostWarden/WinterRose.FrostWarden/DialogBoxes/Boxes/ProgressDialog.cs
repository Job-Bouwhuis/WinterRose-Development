using Raylib_cs;
using System.Formats.Tar;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes.Boxes
{
    public class ProgressDialog : Dialog
    {
        public float ProgressValue { get; private set; }

        public ProgressDialog(
            string title, 
            string message,
            float initialProgress,
            DialogPlacement placement = DialogPlacement.CenterSmall,
            DialogPriority priority = DialogPriority.Normal, 
            string[]? buttons = null, 
            Action[]? onButtonClick = null, 
            Action<UIContext>? onImGui = null) 
            : base(title, message, placement, priority, buttons ?? [], onButtonClick ?? [], onImGui)
        {
            ProgressValue = initialProgress;
        }

        public override void Update()
        {
            if (!IsClosing)
            {
                AnimationTimeIn = MathF.Min(AnimationTimeIn + Time.deltaTime, Dialogs.ANIMATION_DURATION);
                if (AnimationTimeIn / Dialogs.ANIMATION_DURATION >= 0.9f)
                    ContentAlphaTime = MathF.Min(ContentAlphaTime + Time.deltaTime, Dialogs.CONTENT_FADE_DURATION);
            }
            else
            {
                AnimationTimeOut += Time.deltaTime;
                ContentAlphaTime = MathF.Max(ContentAlphaTime - Time.deltaTime * 2f, 0f);
            }

            // Progress dialog might not have a time-based auto close, but you can add if you want
        }

        public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            Rectangle barBg = new Rectangle(bounds.X + padding, y, innerWidth, 20);
            Rectangle barFillRect = new Rectangle(barBg.X, barBg.Y, innerWidth * ProgressValue, 20);

            ray.DrawRectangleRec(barBg, Style.BarBackground);
            ray.DrawRectangleRec(barFillRect, Style.BarFill);

            string progressText = $"{MathF.Round(ProgressValue * 100f, 1)}%";
            int textWidth = ray.MeasureText(progressText, 14);
            int textX = (int)(barBg.X + (barBg.Width - textWidth) / 2);
            int textY = (int)(barBg.Y + 2); // a little padding from top

            Color progressTextColor = new Color(255, 255, 255, (int)(255 * contentAlpha));
            ray.DrawText(progressText, textX, textY, 14, progressTextColor);

            y += 30;
        }
    }
}