using BulletSharp.SoftBody;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface;
public class UICircleProgress : UIContent
{
    public float ProgressValue { get; private set; } = 0f;
    public Func<float, float>? ProgressProvider { get; }
    public string Text { get; }

    private float visualProgress = 0f;
    private float percentStart;
    private float percentTarget;
    private float displayedPercent = 0f;

    // Visual constants
    protected internal const int Padding = 6;
    public float CycleDuration { get; set; } = 1.2f;
    /// <summary>
    /// smallest arc when "shrunk"
    /// </summary>
    public float MinSweepDeg { get; set; } = 25f;
    /// <summary>
    /// largest arc when "expanded"
    /// </summary>
    public float MaxSweepDeg { get; set; } = 180f;
    /// <summary>
    /// How many segments the circle uses to draw itself
    /// </summary>
    public int Segments { get; set; } = 48;
    /// <summary>
    /// ring thickness relative to radius (0..1)
    /// </summary>
    public float ThicknessRatio { get; set; } = 0.10f;

    public bool AlwaysShowText { get; set; } = false; // if true, infinite text will be shown even when determinate
    public float ProgressLerpSpeed { get; set; } = 6f;       // how fast visual progress follows ProgressValue
    public float TextLerpSpeed { get; set; } = 12f;          // how fast displayed percent follows visualProgress
    private Curve PercentCurve { get; set; } = Curves.ExtraSlowFastSlow;

    // Animation state for indeterminate spinner
    private float spinnerRotationDeg = 0f;  // current rotation (degrees)
    private float spinnerPhaseT = 0f;       // 0..1 phase inside one expand/shrink cycle
    private bool spinnerExpanding = true;   // whether sweep is increasing
    private float currentSweepDeg;
    private bool transitioningToDeterminate = false;
    private float transitionT = 0f;
    private float percentTransitionT = 0;
    public float StateTransitionDuration { get; set; } = 0.05f;
    private float capturedStartDeg = 0f;
    private float capturedEndDeg = 0f;
    private float targetEndDeg = 0f;
    private bool wasIndeterminate = false;
    private bool transitioningToIndeterminate = false;
    private float transitionToIndeterminateT = 0f;

    // derived geometry each frame
    private float outerRadius;
    private float innerRadius;
    private Vector2 center;
    private float visualProgressStart;
    private float visualProgressTarget;
    private float visualProgressTransitionT;

    public UICircleProgress(float initialProgress = 0f, Func<float, float>? progressProvider = null, string infiniteSpinText = "Working...")
    {
        currentSweepDeg = MinSweepDeg;
        ProgressValue = Math.Clamp(initialProgress, -1f, 1f);
        ProgressProvider = progressProvider;
        Text = infiniteSpinText;
    }

    public void SetProgress(float value)
    {
        ProgressValue = Math.Clamp(value, 0f, 1f);
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // Make the control square and as large as possible inside available area, keep a minimum sensible size
        int minSide = Math.Min((int)availableArea.Width, (int)availableArea.Height);
        minSide = Math.Max(minSide, 30); // minimum
        return new Vector2(minSide, minSide);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        // height equals chosen square side
        return GetSize(new Rectangle(0, 0, (int)maxWidth, (int)maxWidth)).Y;
    }

    protected internal override void Setup()
    {
        // initialize animation values
        spinnerRotationDeg = 0f;
        spinnerPhaseT = 0f;
        spinnerExpanding = true;
        currentSweepDeg = MinSweepDeg;
        outerRadius = 0f;
        innerRadius = 0f;
        center = Vector2.Zero;
        transitioningToDeterminate = false;
        transitionT = 0f;
        wasIndeterminate = (ProgressValue == -1f);
        transitioningToIndeterminate = false;
        transitionToIndeterminateT = 0f;

        visualProgress = Math.Clamp(ProgressValue >= 0f ? ProgressValue : 0f, 0f, 1f);
        displayedPercent = visualProgress * 100f;
    }

    private float LerpAngleWithSpinnerRange(
    float fromDeg, float toDeg,
    float spinnerStartDeg, float spinnerEndDeg,
    float t,
    float margin = 15f)
    {
        // normalize everything to [0,360)
        fromDeg = NormalizeAngle(fromDeg);
        toDeg = NormalizeAngle(toDeg);
        spinnerStartDeg = NormalizeAngle(spinnerStartDeg);
        spinnerEndDeg = NormalizeAngle(spinnerEndDeg);

        // Helper: is angle within [start,end] accounting for wrapping
        bool AngleInArc(float angle, float start, float end)
        {
            if (end >= start)
                return angle >= start && angle <= end;
            else
                return angle >= start || angle <= end;
        }

        // expand spinner sweep by margin
        float marginStart = NormalizeAngle(spinnerStartDeg - margin);
        float marginEnd = NormalizeAngle(spinnerEndDeg + margin);

        bool withinRange = AngleInArc(toDeg, marginStart, marginEnd);

        float delta = (toDeg - fromDeg) % 360f;
        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;

        if (!withinRange && delta < 0f)
        {
            // force clockwise if target outside allowed range
            delta += 360f;
        }

        return NormalizeAngle(fromDeg + delta * Math.Clamp(t, 0f, 1f));
    }

    private float LerpAngle(float fromDeg, float toDeg, float t, float allowance = 15f)
    {
        // delta in [-180, 180] range
        float delta = (toDeg - fromDeg) % 360f;
        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;

        if (MathF.Abs(delta) <= allowance)
        {
            // take shortest path
        }
        else if (delta < 0f)
        {
            // otherwise, force clockwise motion
            delta += 360f;
        }

        return (fromDeg + delta * Math.Clamp(t, 0f, 1f)) % 360f;
    }

    protected internal override void Update()
    {
        if(visualProgress != 1)
            Style.PauseAutoDismissTimer = true;

        bool prevWasIndeterminate = wasIndeterminate;

        if (ProgressProvider is not null)
            ProgressValue = ProgressProvider(ProgressValue);

        // normalize sentinel for indeterminate: -1 == indeterminate
        if (ProgressValue is < 0f and > -0.1f)
            ProgressValue = 0;
        else if (ProgressValue is not -1 and < -0.1f)
            ProgressValue = -1;

        bool nowIndeterminate = (ProgressValue == -1f);
        float dt = Time.deltaTime;

        // ---------------- Spinner animation ----------------
        if (nowIndeterminate || transitioningToDeterminate || transitioningToIndeterminate)
        {
            const float SPIN_SPEED_DEG_PER_SEC = 360f * 0.8f;
            spinnerRotationDeg = (spinnerRotationDeg + SPIN_SPEED_DEG_PER_SEC * dt) % 360f;

            float phaseDuration = CycleDuration / 2f;
            spinnerPhaseT += dt / phaseDuration;
            if (spinnerPhaseT >= 1f)
            {
                spinnerPhaseT -= 1f;
                spinnerExpanding = !spinnerExpanding;
            }
            float easedPhase = (float)(0.5f - 0.5f * Math.Cos(Math.PI * spinnerPhaseT));
            currentSweepDeg = Lerp(MinSweepDeg, MaxSweepDeg, spinnerExpanding ? easedPhase : (1f - easedPhase));
        }

        // ---------------- Transition: indeterminate -> determinate ----------------
        if (prevWasIndeterminate && !nowIndeterminate)
        {
            transitioningToDeterminate = true;
            transitionT = 0f;

            capturedStartDeg = spinnerRotationDeg;
            capturedEndDeg = spinnerRotationDeg + currentSweepDeg;

            capturedStartDeg = NormalizeAngle(capturedStartDeg);
            capturedEndDeg = NormalizeAngle(capturedEndDeg);

            float clampedTarget = Math.Clamp(ProgressValue, 0f, 1f);
            targetEndDeg = -90f + clampedTarget * 360f;
            targetEndDeg = NormalizeAngle(targetEndDeg);

            float spinnerProgress = NormalizeAngle(capturedEndDeg - (-90f)) / 360f;
            visualProgress = Math.Clamp(spinnerProgress, 0f, 1f);

            // reset percent curve
            percentStart = displayedPercent;
            percentTarget = visualProgress * 100f;
            percentTransitionT = 0f;
        }

        // ---------------- Transition: determinate -> indeterminate ----------------
        if (!prevWasIndeterminate && nowIndeterminate)
        {
            transitioningToIndeterminate = true;
            transitionToIndeterminateT = 0f;

            capturedStartDeg = -90f;
            capturedEndDeg = -90f + Math.Clamp(visualProgress, 0f, 1f) * 360f;

            capturedStartDeg = NormalizeAngle(capturedStartDeg);
            capturedEndDeg = NormalizeAngle(capturedEndDeg);

            spinnerRotationDeg = NormalizeAngle(capturedStartDeg);
            spinnerPhaseT = 0f;
            spinnerExpanding = true;
            currentSweepDeg = MinSweepDeg;
        }

        // ---------------- Advance transition to determinate ----------------
        if (transitioningToDeterminate)
        {
            transitionT += dt / Math.Max(0.0001f, StateTransitionDuration);
            float easeT = (float)(0.5f - 0.5f * Math.Cos(Math.PI * Math.Clamp(transitionT, 0f, 1f)));

            // visualProgress handled in Draw via interpolation if needed
            if (transitionT >= 1f)
            {
                transitioningToDeterminate = false;
                visualProgress = Math.Clamp(ProgressValue, 0f, 1f);
            }

            // also drive percent during transition using curve
            percentTransitionT = Math.Clamp(transitionT, 0f, 1f);
            displayedPercent = Lerp(percentStart, percentTarget, PercentCurve.Evaluate(percentTransitionT));
        }

        // ---------------- Advance transition to indeterminate ----------------
        if (transitioningToIndeterminate)
        {
            transitionToIndeterminateT += dt / Math.Max(0.0001f, StateTransitionDuration);
            float t = Math.Clamp(transitionToIndeterminateT, 0f, 1f);

            percentTransitionT = t; // you can optionally curve this as well
            displayedPercent = Lerp(percentStart, percentTarget, PercentCurve.Evaluate(percentTransitionT));

            if (transitionToIndeterminateT >= 1f)
            {
                transitioningToIndeterminate = false;
            }
        }

        // ---------------- Normal determinate progress ----------------
        if (!nowIndeterminate && !transitioningToDeterminate)
        {
            // detect if target changed significantly
            float clampedTarget = Math.Clamp(ProgressValue, 0f, 1f);
            if (Math.Abs(clampedTarget - visualProgressTarget) > 0.001f)
            {
                visualProgressStart = visualProgress;
                visualProgressTarget = clampedTarget;
                visualProgressTransitionT = 0f;
            }

            // advance transition
            visualProgressTransitionT += dt * ProgressLerpSpeed;
            visualProgressTransitionT = Math.Clamp(visualProgressTransitionT, 0f, 1f);

            // apply curve to visual progress
            float curveT = PercentCurve.Evaluate(visualProgressTransitionT);
            visualProgress = Lerp(visualProgressStart, visualProgressTarget, curveT);

            // displayed percent follows visual progress (also can use same curve)
            displayedPercent = visualProgress * 100f;
        }


        wasIndeterminate = nowIndeterminate;
    }

    protected override void Draw(Rectangle bounds)
    {
        float side = Math.Min(bounds.Width, bounds.Height);
        float half = side / 2f;

        center = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        outerRadius = half - Padding;
        outerRadius = Math.Max(outerRadius, 8f);
        innerRadius = outerRadius * (1f - ThicknessRatio);
        innerRadius = Math.Max(innerRadius, 3f);

        Color bgRing = Style.ProgressBarBackground;
        Color fill = Style.ProgressBarFill;
        Color textColor = Style.White;

        // Draw background ring as a stroked arc (full 360)
        float midRadius = (outerRadius + innerRadius) * 0.5f;
        float thickness = outerRadius - innerRadius;
        DrawThickArc(center, midRadius, thickness, 0f, 360f, Segments, bgRing);

        // Determine sweep to draw from visualProgress when determinate
        if (transitioningToDeterminate)
        {
            float eased = (float)(0.5f - 0.5f * Math.Cos(Math.PI * Math.Clamp(transitionT, 0f, 1f)));
            float drawStart = LerpAngle(capturedStartDeg, -90f, eased);
            float drawEnd = LerpAngleWithSpinnerRange(
                capturedEndDeg,
                targetEndDeg,
                spinnerRotationDeg,
                spinnerRotationDeg + currentSweepDeg,
                eased,
                15f
            );
            DrawThickArc(center, midRadius, thickness, drawStart, drawEnd, Segments, fill);
        }
        else if (transitioningToIndeterminate)
        {
            // interpolate from captured determinate arc into the *live* spinner arc
            float t = Math.Clamp(transitionToIndeterminateT, 0f, 1f);
            float eased = (float)(0.5f - 0.5f * Math.Cos(Math.PI * t));

            // spinnerRotationDeg & currentSweepDeg are being updated during transition, so interpolate to their current live values
            float drawStart = LerpAngle(capturedStartDeg, NormalizeAngle(spinnerRotationDeg), eased);
            float drawEnd = LerpAngleWithSpinnerRange(
                capturedEndDeg,
                targetEndDeg,
                spinnerRotationDeg,
                spinnerRotationDeg + currentSweepDeg,
                eased,
                15f
            );

            DrawThickArc(center, midRadius, thickness, drawStart, drawEnd, Segments, fill);
        }
        else if (ProgressValue != -1)
        {
            float clamped = Math.Clamp(visualProgress, 0f, 1f);
            float startDeg = -90f;
            float sweep = clamped * 360f;
            float endDeg = startDeg + sweep;

            if (sweep > 0.001f)
                DrawThickArc(center, midRadius, thickness, startDeg, endDeg, Segments, fill);
        }
        else
        {
            float startDeg = spinnerRotationDeg;
            float endDeg = spinnerRotationDeg + currentSweepDeg;
            DrawThickArc(center, midRadius, thickness, startDeg, endDeg, Segments, fill);
        }

        // Draw inner text with space-aware logic:
        // - InfiniteSpinText is shown when indeterminate OR AlwaysShowInfiniteText is true (and text non-empty)
        // - Percent text is shown when determinate
        // - If both would be shown but don't fit, percent text is hidden (infinite text has priority)
        string? infiniteTextToShow = (ProgressValue == -1 || AlwaysShowText) && !string.IsNullOrWhiteSpace(Text)
            ? Text
            : null;

        bool percentEligible = ProgressValue != -1;
        string percentText = $"{displayedPercent:F1}%";
        if (!percentEligible)
            percentText = null;

        int fontSize = Math.Clamp((int)(innerRadius * 0.6f), 10, 28);

        // available inner area for text
        float innerDiameter = innerRadius * 2f;
        float paddingInside = 8f; // small breathing room

        // measure widths (ray.MeasureText returns int)
        int infWidth = infiniteTextToShow is null ? 0 : ray.MeasureText(infiniteTextToShow, fontSize);
        int percWidth = percentText is null ? 0 : ray.MeasureText(percentText, fontSize);

        bool drawInfinite = infiniteTextToShow is not null;
        bool drawPercent = percentText is not null;

        // If both would be drawn, check fit. percent disappears first if there's no room.
        if (drawInfinite && drawPercent)
        {
            float combinedWidth = infWidth + percWidth + 6f; // spacing between
            if (combinedWidth + paddingInside > innerDiameter)
            {
                // not enough space: hide percent
                drawPercent = false;
            }
        }

        // Choose how to layout text:
        if (drawInfinite && drawPercent)
        {
            // draw both side-by-side centered
            float totalW = infWidth + percWidth + 6f;
            float leftX = center.X - totalW / 2f;

            ray.DrawText(infiniteTextToShow, (int)leftX, (int)(center.Y - fontSize / 2f), fontSize, textColor);
            ray.DrawText(percentText, (int)(leftX + infWidth + 6f), (int)(center.Y - fontSize / 2f), fontSize, textColor);
        }
        else if (drawInfinite)
        {
            float textWidth = infWidth;
            float textX = center.X - textWidth / 2f;
            ray.DrawText(infiniteTextToShow, (int)textX, (int)(center.Y - fontSize / 2f), fontSize, textColor);
        }
        else if (drawPercent)
        {
            float textWidth = percWidth;
            float textX = center.X - textWidth / 2f;
            ray.DrawText(percentText, (int)textX, (int)(center.Y - fontSize / 2f), fontSize, textColor);
        }
    }

    // new helper: draws a stroked (thick) arc centered at `center` with `midRadius` and `thickness`
    // draws only the requested angular range [startDeg, endDeg], triangulated into quads.
    private void DrawThickArc(Vector2 center, float midRadius, float thickness, float startDeg, float endDeg, int segments, Color color)
    {
        const float DEG2RAD = MathF.PI / 180f;

        float sweepDeg = endDeg - startDeg;
        if (sweepDeg < 0f) sweepDeg += 360f;
        if (sweepDeg <= 0.001f) return;

        int segs = Math.Max(3, (int)MathF.Ceiling(segments * (sweepDeg / 360f)));
        float halfThickness = Math.Max(0.5f, thickness * 0.5f);

        float startRad = startDeg * DEG2RAD;
        float sweepRad = sweepDeg * DEG2RAD;
        float angleStep = sweepRad / segs;

        Vector2[] outerPts = new Vector2[segs + 1];
        Vector2[] innerPts = new Vector2[segs + 1];

        for (int i = 0; i <= segs; i++)
        {
            float ang = startRad + i * angleStep;
            float cos = MathF.Cos(ang);
            float sin = MathF.Sin(ang);

            Vector2 normal = new Vector2(cos, sin);
            Vector2 mid = center + normal * midRadius;

            outerPts[i] = mid + normal * halfThickness;
            innerPts[i] = mid - normal * halfThickness;
        }

        // Triangles (ensure correct winding for Raylib screen coordinates)
        for (int i = 0; i < segs; i++)
        {
            Vector2 innerA = innerPts[i];
            Vector2 outerA = outerPts[i];
            Vector2 outerB = outerPts[i + 1];
            Vector2 innerB = innerPts[i + 1];

            ray.DrawTriangle(outerA, innerA, outerB, color);
            ray.DrawTriangle(innerA, innerB, outerB, color);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }



}

