using BulletSharp.SoftBody;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface;
public class UICircleProgress : UIContent
{
    public float ProgressValue { get; private set; } = 0f;
    public Func<UICircleProgress, float, float>? ProgressProvider { get; set; }
    public string Text { get; set; }

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
    public bool DontShowProgressPercent { get; set; }

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

    public UICircleProgress(float initialProgress = 0f, Func<UICircleProgress, float, float>? progressProvider = null, string infiniteSpinText = "Working...")
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
        // Determine maximum square side
        float side = Math.Min(availableArea.Width, availableArea.Height);
        side = Math.Max(side, 30); // enforce minimum

        // compute radii like in Draw
        float half = side / 2f;
        float outerRadiusLocal = half - Padding;
        outerRadiusLocal = Math.Max(outerRadiusLocal, 8f);
        float innerRadiusLocal = outerRadiusLocal * (1f - ThicknessRatio);
        innerRadiusLocal = Math.Max(innerRadiusLocal, 3f);

        // Determine text height
        int fontSize = Math.Clamp((int)innerRadiusLocal, 10, 28);
        float textHeight = fontSize; // rough approximation

        // Add bottom padding for text (so it fits inside the container)
        float totalHeight = outerRadiusLocal * 2f + textHeight;
        float totalWidth = outerRadiusLocal * 2f;

        return new Vector2(totalWidth, totalHeight);
    }

    protected internal override float GetHeight(float maxWdith)
    {
        maxWdith = Math.Max(maxWdith, 30);

        float half = maxWdith / 2f;
        float outerRadiusLocal = half - Padding;
        outerRadiusLocal = Math.Max(outerRadiusLocal, 8f);

        float innerRadiusLocal = outerRadiusLocal * (1f - ThicknessRatio);
        innerRadiusLocal = Math.Max(innerRadiusLocal, 3f);

        int fontSize = Math.Clamp((int)innerRadiusLocal, 10, 28);
        float textHeight = fontSize;

        return outerRadiusLocal * 2f + textHeight;
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
        bool prevWasIndeterminate = wasIndeterminate;

        if (ProgressProvider is not null)
            ProgressValue = ProgressProvider(this, ProgressValue);

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

        // Draw ring arcs as before...
        DrawProgressArc();

        // --- Text layout calculation (instead of drawing via Raylib) ---
        string? infiniteTextToShow = (ProgressValue == -1 || AlwaysShowText) && !string.IsNullOrWhiteSpace(Text)
            ? Text
            : null;

        bool percentEligible = ProgressValue != -1;
        string? percentText = percentEligible && !DontShowProgressPercent ? $"{displayedPercent:F1}%" : null;

        int fontSize = Math.Clamp((int)(innerRadius), 10, 28);
        float innerDiameter = innerRadius * 2f;
        float paddingInside = 8f;

        // Measure text widths using your measurement method

        RichText? infText = null;
        if(infiniteTextToShow is not null)
        {
            infText = infiniteTextToShow;
            infText.FontSize = fontSize;
        }

        RichText? percText = null;
        if (percentText is not null)
        {
            percText = percentText!;
            percText.FontSize = fontSize;
        }

        float infWidth = infiniteTextToShow is null ? 0 : infText.MeasureText(null);
        float percWidth = percentText is null ? 0 : percText.MeasureText(null);

        bool drawInfinite = infiniteTextToShow is not null;
        bool drawPercent = percentText is not null;

        if (drawInfinite && drawPercent)
        {
            float combinedWidth = infWidth + percWidth + 6f; // spacing between
            if (combinedWidth + paddingInside > innerDiameter)
                drawPercent = false; // hide percent first
        }

        // Compute positions for your own renderer
        if (drawInfinite && drawPercent)
        {
            float totalWidth = infWidth + percWidth + 6f;
            Vector2 infPos = new(center.X - totalWidth / 2f, center.Y - fontSize / 2f);
            Vector2 percPos = new(infPos.X + infWidth + 6f, center.Y - fontSize / 2f);

            RichTextRenderer.DrawRichText(infText, infPos, totalWidth, null);
            RichTextRenderer.DrawRichText(percText, percPos, totalWidth, null);
            
        }
        else if (drawInfinite)
        {
            Vector2 infPos = new(center.X - infWidth / 2f, center.Y - fontSize / 2f);

            RichTextRenderer.DrawRichText(infText, infPos, bounds.Width - infPos.X, null);
        }
        else if (drawPercent)
        {
            Vector2 percPos = new(center.X - percWidth / 2f, center.Y - fontSize / 2f);

            RichTextRenderer.DrawRichText(infText, percPos, bounds.Width - percPos.X, null);
        }
    }

    private void DrawProgressArc()
    {
        float midRadius = (outerRadius + innerRadius) * 0.5f;
        float thickness = outerRadius - innerRadius;

        // Background ring (full 360°)
        DrawThickArc(center, midRadius, thickness, 0f, 360f, Segments, Style.ProgressBarBackground);

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

            DrawThickArc(center, midRadius, thickness, drawStart, drawEnd, Segments, Style.ProgressBarFill);
        }
        else if (transitioningToIndeterminate)
        {
            float t = Math.Clamp(transitionToIndeterminateT, 0f, 1f);
            float eased = (float)(0.5f - 0.5f * Math.Cos(Math.PI * t));

            float drawStart = LerpAngle(capturedStartDeg, NormalizeAngle(spinnerRotationDeg), eased);
            float drawEnd = LerpAngleWithSpinnerRange(
                capturedEndDeg,
                targetEndDeg,
                spinnerRotationDeg,
                spinnerRotationDeg + currentSweepDeg,
                eased,
                15f
            );

            DrawThickArc(center, midRadius, thickness, drawStart, drawEnd, Segments, Style.ProgressBarFill);
        }
        else if (ProgressValue != -1)
        {
            float clamped = Math.Clamp(visualProgress, 0f, 1f);
            float startDeg = -90f;
            float sweep = clamped * 360f;
            float endDeg = startDeg + sweep;

            if (sweep > 0.001f)
                DrawThickArc(center, midRadius, thickness, startDeg, endDeg, Segments, Style.ProgressBarFill);
        }
        else
        {
            float startDeg = spinnerRotationDeg;
            float endDeg = spinnerRotationDeg + currentSweepDeg;
            DrawThickArc(center, midRadius, thickness, startDeg, endDeg, Segments, Style.ProgressBarFill);
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

