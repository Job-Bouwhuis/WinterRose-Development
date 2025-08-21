using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public class UIProgressContent : UIContent
{
    public float ProgressValue { get; set; }
    public Func<float, float>? ProgressProvider { get; }
    public string InfiniteSpinText { get; }

    private float cycleDuration = 2f;

    private enum IndetPhase
    {
        GrowingLeft,
        MovingRight,
        ShrinkingRight,
        GrowingRight,
        MovingLeft,
        ShrinkingLeft
    }

    private IndetPhase phase = IndetPhase.GrowingLeft;
    private float segWidth = 0f;
    private float visualLeft = 0f;
    private float maxWidth;
    private float growSpeed, moveSpeed, shrinkSpeed;
    private float visualRight = 0f; // actually the right edge of bar
    private readonly bool closesToastWhenComplete;

    public UIProgressContent(float initialProgress = 0f, Func<float, float>? ProgressProvider = null, bool closesToastWhenComplete = true, string infiniteSpinText = "Working...")
    {
        ProgressValue = Math.Clamp(initialProgress, -1f, 1f);
        this.ProgressProvider = ProgressProvider;
        this.closesToastWhenComplete = closesToastWhenComplete;
        InfiniteSpinText = infiniteSpinText;
    }

    public void SetProgress(float value)
    {
        ProgressValue = Math.Clamp(value, 0f, 1f);
    }

    protected internal override float GetHeight(float width) => 30f;

    public override Vector2 GetSize(Rectangle availableArea)
    {
        Vector2 availableSize = availableArea.Size;
        return availableSize with
        {
            Y = 30
        };
    }

    protected internal override void Update()
    {
        if (ProgressProvider is not null)
            ProgressValue = ProgressProvider(ProgressValue);

        if (ProgressValue is < 0f and > -0.1f)
            ProgressValue = 0;
        else if (ProgressValue is not -1 and < -0.1f)
            ProgressValue = -1;

        if (ProgressValue >= 1 && closesToastWhenComplete)
            Close();
    }

    protected internal override void Draw(Rectangle bounds)
    {
        owner.TimeUntilAutoDismiss = 0;

        Rectangle barBg = new(bounds.X, bounds.Y, bounds.Width, 20);
        maxWidth = barBg.Width * 0.3f;

        float phaseDuration = cycleDuration / 6f;
        growSpeed = moveSpeed = (barBg.Width - maxWidth) / phaseDuration;
        shrinkSpeed = maxWidth / phaseDuration;

        Color bg = Style.ProgressBarBackground;
        Color fill = Style.ProgressBarFill;
        ray.DrawRectangleRec(barBg, bg);

        if (ProgressValue == -1)
        {
            switch (phase)
            {
                case IndetPhase.GrowingLeft:
                    segWidth += growSpeed * Time.deltaTime;
                    visualLeft = barBg.X;
                    if (segWidth >= maxWidth)
                        phase = IndetPhase.MovingRight;
                    break;

                case IndetPhase.MovingRight:
                    visualLeft += moveSpeed * Time.deltaTime;
                    if (visualLeft + segWidth >= barBg.X + barBg.Width)
                        phase = IndetPhase.ShrinkingRight;
                    break;

                case IndetPhase.ShrinkingRight:
                    segWidth -= growSpeed * Time.deltaTime;
                    visualLeft = barBg.X + barBg.Width - segWidth;
                    if (segWidth <= 0f)
                        phase = IndetPhase.GrowingRight;
                    break;

                case IndetPhase.GrowingRight:
                    segWidth += growSpeed * Time.deltaTime;
                    visualLeft = barBg.X + barBg.Width - segWidth;
                    if (segWidth >= maxWidth)
                        phase = IndetPhase.MovingLeft;
                    break;

                case IndetPhase.MovingLeft:
                    visualLeft -= moveSpeed * Time.deltaTime;
                    if (visualLeft <= barBg.X)
                        phase = IndetPhase.ShrinkingLeft;
                    break;

                case IndetPhase.ShrinkingLeft:
                    segWidth -= growSpeed * Time.deltaTime;
                    visualLeft = barBg.X;
                    if (segWidth <= 0f)
                        phase = IndetPhase.GrowingLeft;
                    break;
            }

            // Only compute right edge relative to left
            visualRight = visualLeft + segWidth;

            Rectangle seg = new(visualLeft, barBg.Y, segWidth, barBg.Height);
            ray.DrawRectangleRec(seg, fill);
        }
        else
        {
            // Target edges for actual progress
            float targetLeft = barBg.X;
            float targetRight = barBg.X + barBg.Width * Math.Clamp(ProgressValue, 0f, 1f);

            // Smoothly interpolate from current visual edges to target edges
            float lerpSpeed = (owner.IsClosing ? 100f : 5f) * Time.deltaTime;

            visualLeft = Lerp(visualLeft, targetLeft, lerpSpeed);
            visualRight = Lerp(visualRight, targetRight, lerpSpeed);


            Rectangle barFillRect = new(visualLeft, barBg.Y, visualRight - visualLeft, barBg.Height);
            ray.DrawRectangleRec(barFillRect, fill);
        }

        // Draw progress text
        string progressText = ProgressValue == -1
            ? InfiniteSpinText
            : $"{MathF.Round(ProgressValue * 100f, 1)}%";

        int fontSize = Math.Clamp((int)(barBg.Height * 0.7f), 12, 20);
        int textWidth = ray.MeasureText(progressText, fontSize);
        float textX = barBg.X + (barBg.Width - textWidth) / 2;
        float textY = barBg.Y + (barBg.Height - fontSize) / 2;

        ray.DrawText(progressText, (int)textX, (int)textY, fontSize, new Color(255, 255, 255, (int)(255 * Style.ContentAlpha)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float from, float to, float t) => from + (to - from) * t;

}


