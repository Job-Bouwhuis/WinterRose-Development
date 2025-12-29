using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface;
public class UIProgress : UIContent
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
    private float visualRight = 0f;
    private float edgeTransitionT = 1f;
    private float edgeStartLeft = 0f;
    private float edgeStartRight = 0f;
    private float prevTargetLeft = float.NaN;
    private float prevTargetRight = float.NaN;
    public float EdgeTransitionDuration { get; set; } = 0.25f;
    public Curve PercentCurve { get; set; } = Curves.ExtraSlowFastSlow;

    public UIProgress(float initialProgress = 0f, Func<float, float>? ProgressProvider = null, string infiniteSpinText = "Working...")
    {
        ProgressValue = Math.Clamp(initialProgress, -1f, 1f);
        this.ProgressProvider = ProgressProvider;
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
        if (ProgressValue != 1)
            Style.PauseAutoDismissTimer = true;
        if (ProgressProvider is not null)
            ProgressValue = ProgressProvider(ProgressValue);

        if (ProgressValue is < 0f and > -0.1f)
            ProgressValue = 0;
        else if (ProgressValue is not -1 and < -0.1f)
            ProgressValue = -1;
    }

    public void ForceDraw(Rectangle bounds)
    {
        Draw(bounds);
    }

    protected override void Draw(Rectangle bounds)
    {
        owner.Style.TimeUntilAutoDismiss = 0;

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
            // Target edges for actual progress (computed from bounds)
            float targetLeft = barBg.X;
            float targetRight = barBg.X + barBg.Width * Math.Clamp(ProgressValue, 0f, 1f);

            // Detect target change and start a new transition when it changes
            bool targetChanged = float.IsNaN(prevTargetLeft) ||
                                 MathF.Abs(targetLeft - prevTargetLeft) > 0.5f ||
                                 MathF.Abs(targetRight - prevTargetRight) > 0.5f;

            if (targetChanged)
            {
                prevTargetLeft = targetLeft;
                prevTargetRight = targetRight;

                // capture current visual edges as start points
                edgeStartLeft = visualLeft;
                edgeStartRight = visualRight;

                // restart transition
                edgeTransitionT = 0f;
            }

            // Advance transition (uses Time.deltaTime so it's frame-rate independent)
            if (edgeTransitionT < 1f)
            {
                edgeTransitionT += Time.deltaTime / Math.Max(0.0001f, EdgeTransitionDuration);
                float easedT = PercentCurve.Evaluate(Math.Clamp(edgeTransitionT, 0f, 1f));

                visualLeft = Lerp(edgeStartLeft, targetLeft, easedT);
                visualRight = Lerp(edgeStartRight, targetRight, easedT);

                if (edgeTransitionT >= 1f)
                {
                    visualLeft = targetLeft;
                    visualRight = targetRight;
                }
            }
            else
            {
                // finished — keep exact targets
                visualLeft = targetLeft;
                visualRight = targetRight;
            }

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

        ray.DrawText(progressText, (int)textX, (int)textY, fontSize, Style.White);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float from, float to, float t) => from + (to - from) * t;

}


