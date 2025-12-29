using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface;
public class UIGraph : UIContent
{
    private const float AXIS_MARGIN = 32f;
    private const float LEGEND_HEIGHT = 24f;
    private const float POINT_HIT_RADIUS = 6f;
    private const float DEFAULT_BAR_WIDTH = 12f;
    private const int GRAPH_TEXT_SIZE = 18;

    private const float TICK_LENGTH = 8f;
    private const float LABEL_MARGIN = 0f;

    private readonly List<Series> series = new();
    private Vector2 pan = new(0, 0);
    private bool isPanning;
    private Vector2 panStart;
    private (int seriesIndex, int pointIndex)? selectedPoint = null;

    public float YLerpSpeed { get; set; } = 0.1f;  // how fast Y-axis adapts

    // NEW: cap for requested height (0 = no cap)
    public float MaxHeight { get; set; } = 0f;

    /// <summary>Tick interval along X in data units. Set >0 to force a fixed interval. Set to 0 for automatic spacing.</summary>
    public float XTickInterval { get; set; } = 0f;

    /// <summary>Tick interval along Y in data units. Set >0 to force a fixed interval. Set to 0 for automatic spacing.</summary>
    public float YTickInterval { get; set; } = 0f;

    /// <summary>Used when interval is automatic: approximate number of ticks.</summary>
    public int ApproxTickCount { get; set; } = DEFAULT_TICK_COUNT;

    public string XAxisLabel { get; set; } = "";
    public string YAxisLabel { get; set; } = "";

    private const int DEFAULT_TICK_COUNT = 5;

    public int MaxDataPoints { get; set; } = 1000;

    private float currentMinY = 0f;
    private float currentMaxY = 1f;

    public class Series
    {
        public List<Vector2> Points { get; } = new();
        public Color Color { get; set; } = Color.White;
        public bool Visible { get; set; } = true;
        public float BarWidth { get; set; } = DEFAULT_BAR_WIDTH;
        public string Name { get; set; }
        public SeriesType Type { get; set; }

        // max points visible per series

        public int MaxDataPoints { get; set; }

        public Series(string Name, int maxDataPoints, SeriesType Type)
        {
            this.Name = Name;
            this.Type = Type;
            this.MaxDataPoints = maxDataPoints;
        }

        public void AddPoint(float x, float y)
        {
            Points.Add(new Vector2(x, y));
        }

        public void AddPointSliding(float y)
        {
            // Use index as X, X doesn't change dynamically
            float x = Points.Count;
            Points.Add(new Vector2(x, y));

            // Keep only MaxDataPoints
            if (Points.Count > MaxDataPoints)
            {
                Points.RemoveAt(0);
                // Reindex X values so they stay 0..MaxDataPoints-1
                for (int i = 0; i < Points.Count; i++)
                    Points[i] = new Vector2(i, Points[i].Y);
            }
        }
    }

    public enum SeriesType
    {
        Line,
        Bar
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // Ask for our preferred height, but respect available area and MaxHeight
        float desiredHeight = GetHeight(availableArea.Width);

        if (MaxHeight > 0f)
            desiredHeight = Math.Min(desiredHeight, MaxHeight);

        float height = Math.Min(desiredHeight, availableArea.Height);

        return new Vector2(availableArea.Width, height);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        // Mirror the vertical space consumed by Draw: plot aspect + reserved bottom area + margins
        float bottomReserved = LEGEND_HEIGHT + GRAPH_TEXT_SIZE + LABEL_MARGIN * 6;
        float baseHeight = (maxWidth * 0.5f) + bottomReserved + (AXIS_MARGIN * 2f) + 8f;

        if (MaxHeight > 0f)
            baseHeight = Math.Min(baseHeight, MaxHeight);

        return baseHeight;
    }

    private bool TryGetVisibleYBounds(out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        bool hasPoints = false;

        foreach (var series in series)
        {
            int start = Math.Max(0, series.Points.Count - MaxDataPoints);
            for (int i = start; i < series.Points.Count; i++)
            {
                var y = series.Points[i].Y;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
                hasPoints = true;
            }
        }

        if (!hasPoints)
            return false;

        // Add a tiny padding to prevent degenerate cases
        if (Math.Abs(maxY - minY) < 0.0001f)
        {
            minY -= 1;
            maxY += 1;
        }

        return true;
    }


    protected override void Draw(Rectangle bounds)
    {
        // Background
        ray.DrawRectangleRec(bounds, Style.PanelBackground);
        ray.DrawRectangleLinesEx(bounds, 1f, Style.PanelBorder);

        // Inner plotting rect (leave margin for axes and legend)
        float bottomReserved = LEGEND_HEIGHT + GRAPH_TEXT_SIZE + LABEL_MARGIN * 6;
        var plotRect = new Rectangle(
            bounds.X + AXIS_MARGIN,
            bounds.Y + AXIS_MARGIN,
            bounds.Width - AXIS_MARGIN * 2f,
            bounds.Height - AXIS_MARGIN * 2f - bottomReserved
        );

        if (plotRect.Height < 16f)
            plotRect.Height = 16f;

        // Clip region: draw inside plotRect only
        ray.DrawRectangleRec(plotRect, Style.PanelBackgroundDarker);

        // Compute data min/max
        if (!TryGetDataBounds(out var dataMin, out var dataMax))
        {
            // Nothing to draw - write "no data"
            var text = "No data";
            ray.DrawText(text, (int)(plotRect.X + plotRect.Width / 2 - ray.MeasureText(text, GRAPH_TEXT_SIZE) / 2), (int)(plotRect.Y + plotRect.Height / 2 - 8), GRAPH_TEXT_SIZE, Style.TextBoxText);
            DrawLegend(bounds);
            return;
        }

        // Apply zoom and pan to data-space transform
        var worldMin = dataMin;
        var worldMax = dataMax;

        // Expand if degenerate
        if (Math.Abs(worldMax.X - worldMin.X) < 0.0001f)
        {
            worldMin = new Vector2(worldMin.X - 1, worldMin.Y);
            worldMax = new Vector2(worldMax.X + 1, worldMax.Y);
        }
        if (Math.Abs(worldMax.Y - worldMin.Y) < 0.0001f)
        {
            worldMin = new Vector2(worldMin.X, worldMin.Y - 1);
            worldMax = new Vector2(worldMax.X, worldMax.Y + 1);
        }

        if (TryGetVisibleYBounds(out float visibleMinY, out float visibleMaxY))
        {
            float padding = (visibleMaxY - visibleMinY) * 0.05f;
            visibleMinY -= padding;
            visibleMaxY += padding;

            // Only expand immediately
            if (visibleMinY < currentMinY)
                currentMinY = visibleMinY;
            // Only shrink smoothly
            else if (visibleMinY > currentMinY)
                currentMinY += (visibleMinY - currentMinY) * YLerpSpeed;

            if (visibleMaxY > currentMaxY)
                currentMaxY = visibleMaxY;
            else if (visibleMaxY < currentMaxY)
                currentMaxY += (visibleMaxY - currentMaxY) * YLerpSpeed;

            worldMin.Y = currentMinY;
            worldMax.Y = currentMaxY;
        }


        {
            float visibleMinX = dataMin.X;
            float visibleMaxX = dataMax.X;
            float xRange = visibleMaxX - visibleMinX;
            if (Math.Abs(xRange) < 0.0001f)
            {
                visibleMinX -= 1f;
                visibleMaxX += 1f;
                xRange = visibleMaxX - visibleMinX;
            }
            float xPadding = xRange * 0.05f;
            worldMin.X = visibleMinX - xPadding;
            worldMax.X = visibleMaxX + xPadding;
        }

        // Decide axisY to align with the lowest Y grid line (or 0 if 0 is inside range)
        float axisY = (worldMin.Y <= 0f && worldMax.Y >= 0f) ? 0f : GetLowestYTick(worldMin, worldMax);

        // Convert data -> pixel
        Vector2 DataToPixel(Vector2 data)
        {
            float nx = (data.X - worldMin.X) / (worldMax.X - worldMin.X);
            float px = plotRect.X + nx * plotRect.Width;

            float ny = (data.Y - worldMin.Y) / (worldMax.Y - worldMin.Y);
            float py = plotRect.Y + plotRect.Height - ny * plotRect.Height;

            return new Vector2(px, py);
        }

        Vector2 PixelToData(Vector2 pixel)
        {
            float nx = (pixel.X - plotRect.X) / plotRect.Width;
            float ny = 1f - (pixel.Y - plotRect.Y) / plotRect.Height;

            float x = worldMin.X + nx * (worldMax.X - worldMin.X);
            float y = worldMin.Y + ny * (worldMax.Y - worldMin.Y);
            return new Vector2(x, y);
        }

        // Draw grid lines (simple)
        DrawGrid(plotRect, worldMin, worldMax, DataToPixel);

        // Draw series
        for (int i = 0; i < series.Count; i++)
        {
            var s = series[i];
            if (!s.Visible || s.Points.Count == 0)
                continue;

            if (s.Type == SeriesType.Line)
            {
                // Draw polyline
                for (int p = 0; p < s.Points.Count - 1; p++)
                {
                    var a = DataToPixel(s.Points[p]);
                    var b = DataToPixel(s.Points[p + 1]);
                    ray.DrawLineEx(a, b, 2f, s.Color);
                }

                // Draw points
                for (int p = 0; p < s.Points.Count; p++)
                {
                    var pos = DataToPixel(s.Points[p]);
                    ray.DrawCircleV(pos, 3f, s.Color);
                }
            }
            else if (s.Type == SeriesType.Bar)
            {
                // Bars baseline aligned to axisY (so it sits on the bottom grid line)
                float baseY = axisY;
                var basePixel = DataToPixel(new Vector2(0, baseY));

                for (int p = 0; p < s.Points.Count; p++)
                {
                    var dataPoint = s.Points[p];
                    var centerPx = DataToPixel(new Vector2(dataPoint.X, baseY)).X; // center X position for this x
                    float halfW = s.BarWidth * 0.5f;
                    var topPx = DataToPixel(new Vector2(dataPoint.X, dataPoint.Y)).Y;
                    var rect = new Rectangle(centerPx - halfW, Math.Min(topPx, basePixel.Y), halfW * 2, Math.Abs(basePixel.Y - topPx));
                    ray.DrawRectangleRec(rect, s.Color);
                    ray.DrawRectangleLinesEx(rect, 1f, Style.PanelBorder);
                }
            }
        }

        // Handle hover, selection and pan/zoom input
        HandleInput(plotRect, DataToPixel, PixelToData);

        // If a point is selected, show tooltip
        if (selectedPoint.HasValue)
        {
            var (si, pi) = selectedPoint.Value;
            if (si >= 0 && si < series.Count && pi >= 0 && pi < series[si].Points.Count)
            {
                var sp = series[si].Points[pi];
                var pPx = DataToPixel(sp);
                var tip = $"{series[si].Name} ({sp.X:0.###}, {sp.Y:0.###})";
                var tw = ray.MeasureText(tip, GRAPH_TEXT_SIZE);

                float tipW = tw + 8;
                float tipH = 20;
                float tipX = pPx.X + 8;
                float tipY = pPx.Y - 16;

                // Flip to left side if overflowing to the right
                if (tipX + tipW > plotRect.X + plotRect.Width)
                    tipX = pPx.X - 8 - tipW;

                // Ensure it doesn't go past the left edge
                if (tipX < plotRect.X)
                    tipX = plotRect.X;

                // Clamp vertically to stay inside plotRect
                if (tipY < plotRect.Y)
                    tipY = plotRect.Y;
                if (tipY + tipH > plotRect.Y + plotRect.Height)
                    tipY = plotRect.Y + plotRect.Height - tipH;

                var tipRect = new Rectangle(tipX, tipY, tipW, tipH);
                ray.DrawRectangleRec(tipRect, Style.TooltipBackground);
                ray.DrawText(tip, (int)(tipRect.X + 4), (int)(tipRect.Y + 3), GRAPH_TEXT_SIZE, Style.TextSmall);
            }
        }


        // Axes (draw on top)
        DrawAxes(plotRect, worldMin, worldMax, bounds, DataToPixel, axisY);

        DrawLegend(bounds);
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        selectedPoint = null;
        base.OnClickedOutsideOfContent(button);

    }

    protected internal override void OnHover()
    {
    }

    protected internal override void OnHoverEnd()
    {
    }

    protected internal override void OnOwnerClosing()
    {
        base.OnOwnerClosing();
    }

    protected internal override void Setup()
    {
    }

    protected internal override void Update()
    {

    }

    // --- Public API for adding data ---

    public void AddLineSeries(string name, IEnumerable<Vector2> points, Color? color = null)
    {
        var s = new Series(name, MaxDataPoints, SeriesType.Line) { Color = color ?? Color.White };
        s.Points.AddRange(points);
        series.Add(s);
    }

    public void AddValueToSeries(string seriesName, float value, Color? color = null)
    {
        var s = GetSeries(seriesName, color);
        s.AddPointSliding(value);
    }

    public void AddBarSeries(string name, IEnumerable<Vector2> points, Color? color = null, float barWidth = DEFAULT_BAR_WIDTH)
    {
        var s = new Series(name, MaxDataPoints, SeriesType.Bar) { Color = color ?? Color.White, BarWidth = barWidth };
        s.Points.AddRange(points);
        series.Add(s);
    }

    public void AddSeries(Series series) => this.series.Add(series);

    public void ClearSeries()
    {
        series.Clear();
        selectedPoint = null;
    }

    private bool TryGetDataBounds(out Vector2 min, out Vector2 max)
    {
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);
        bool any = false;
        foreach (var s in series)
        {
            if (!s.Visible) continue;
            foreach (var p in s.Points)
            {
                any = true;
                if (p.X < min.X) min = new Vector2(p.X, min.Y);
                if (p.Y < min.Y) min = new Vector2(min.X, p.Y);
                if (p.X > max.X) max = new Vector2(p.X, max.Y);
                if (p.Y > max.Y) max = new Vector2(max.X, p.Y);
            }
        }

        if (!any)
            return false;

        return true;
    }

    private void DrawGrid(Rectangle plotRect, Vector2 worldMin, Vector2 worldMax, Func<Vector2, Vector2> dataToPixel)
    {
        // X grid lines
        if (XTickInterval > 0f)
        {
            float startX = (float)Math.Ceiling(worldMin.X / XTickInterval) * XTickInterval;
            for (float x = startX; x <= worldMax.X + 1e-6f; x += XTickInterval)
            {
                var a = dataToPixel(new Vector2(x, worldMin.Y));
                var b = dataToPixel(new Vector2(x, worldMax.Y));
                ray.DrawLineEx(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y), 1f, Style.GridLine);
            }
        }
        else
        {
            int vLines = Math.Max(1, ApproxTickCount);
            for (int i = 0; i <= vLines; i++)
            {
                float t = i / (float)vLines;
                var dataX = worldMin.X + t * (worldMax.X - worldMin.X);
                var a = dataToPixel(new Vector2(dataX, worldMin.Y));
                var b = dataToPixel(new Vector2(dataX, worldMax.Y));
                ray.DrawLineEx(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y), 1f, Style.GridLine);
            }
        }

        // Y grid lines
        if (YTickInterval > 0f)
        {
            float startY = (float)Math.Ceiling(worldMin.Y / YTickInterval) * YTickInterval;
            for (float y = startY; y <= worldMax.Y + 1e-6f; y += YTickInterval)
            {
                var a = dataToPixel(new Vector2(worldMin.X, y));
                var b = dataToPixel(new Vector2(worldMax.X, y));
                ray.DrawLineEx(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y), 1f, Style.GridLine);
            }
        }
        else
        {
            int hLines = Math.Max(1, ApproxTickCount);
            for (int i = 0; i <= hLines; i++)
            {
                float t = i / (float)hLines;
                var dataY = worldMin.Y + t * (worldMax.Y - worldMin.Y);
                var a = dataToPixel(new Vector2(worldMin.X, dataY));
                var b = dataToPixel(new Vector2(worldMax.X, dataY));
                ray.DrawLineEx(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y), 1f, Style.GridLine);
            }
        }
    }

    private string FormatNumberAbbrev(float value)
    {
        float abs = Math.Abs(value);
        string suffix;
        float divisor;

        switch (abs)
        {
            case >= 1_000_000_000_000f:
                suffix = "t";
                divisor = 1_000_000_000_000f;
                break;
            case >= 1_000_000_000f:
                suffix = "b";
                divisor = 1_000_000_000f;
                break;
            case >= 1_000_000f:
                suffix = "m";
                divisor = 1_000_000f; 
                break;
            case >= 1_000f:
                suffix = "k"; 
                divisor = 1_000f;
                break;
            default:
                suffix = "";
                divisor = 1f;
                break;
        }

        float scaled = value / divisor;

        if (Math.Abs(scaled - (float)Math.Round(scaled)) < 1e-6f)
            return ((int)Math.Round(scaled)).ToString() + suffix;
        return Round(scaled).ToString() + suffix;
    }

    private float Round(float f)
    {
        if (f == 0f)
            return 0f;

        // Determine the magnitude of the number
        float absF = Math.Abs(f);
        float scale = 1f;

        // Reduce absF until it's >= 1, tracking the scale factor
        while (absF < 1f)
        {
            absF *= 10f;
            scale *= 10f;
        }

        // Round to 2 digits at this scale
        float rounded = (float)Math.Floor(f * scale * 100f) / (scale * 100f);

        return rounded;
    }


    // updated method: DrawAxes (full replacement)
    private void DrawAxes(Rectangle plotRect, Vector2 worldMin, Vector2 worldMax, Rectangle bounds, Func<Vector2, Vector2> dataToPixel, float axisY)
    {
        // X axis at y = 0 if visible, else bottom
        var a = dataToPixel(new Vector2(worldMin.X, axisY));
        var b = dataToPixel(new Vector2(worldMax.X, axisY));
        ray.DrawLineEx(a, b, 2f, Style.AxisLine);

        // Y axis at x = 0 if visible, else left
        float axisX = worldMin.X <= 0 && worldMax.X >= 0 ? 0 : worldMin.X;
        var c = dataToPixel(new Vector2(axisX, worldMin.Y));
        var d = dataToPixel(new Vector2(axisX, worldMax.Y));
        ray.DrawLineEx(c, d, 2f, Style.AxisLine);

        // Draw ticks and numeric labels (existing behaviour)
        float axisYPx = a.Y; // pixel Y coordinate of X axis
        if (XTickInterval > 0f)
        {
            float baseStep = XTickInterval;
            int[] multipliers = new[] { 1, 2, 5, 10, 20, 50, 100 };

            bool placed = false;
            foreach (int mult in multipliers)
            {
                float step = baseStep * mult;
                float first = (float)Math.Ceiling(worldMin.X / step) * step;
                float lastLabelRight = float.NegativeInfinity;
                bool collision = false;

                for (float x = first; x <= worldMax.X + 1e-6f; x += step)
                {
                    var px = dataToPixel(new Vector2(x, axisY));
                    var label = FormatNumberAbbrev(x);
                    int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                    float lx = px.X - tw * 0.5f;
                    float labelLeftWithMargin = lx - LABEL_MARGIN;

                    if (labelLeftWithMargin <= lastLabelRight)
                    {
                        collision = true;
                        break;
                    }
                    lastLabelRight = lx + tw + LABEL_MARGIN;
                }

                if (!collision)
                {
                    float start = (float)Math.Ceiling(worldMin.X / (baseStep * mult)) * (baseStep * mult);
                    for (float x = start; x <= worldMax.X + 1e-6f; x += baseStep * mult)
                    {
                        var px = dataToPixel(new Vector2(x, axisY));
                        var t1 = new Vector2(px.X, axisYPx - TICK_LENGTH * 0.5f);
                        var t2 = new Vector2(px.X, axisYPx + TICK_LENGTH * 0.5f);
                        ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                        var label = FormatNumberAbbrev(x);
                        int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                        float lx = px.X - tw * 0.5f;
                        float ly = axisYPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                        ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                    }

                    placed = true;
                    break;
                }
            }


            if (!placed)
            {
                float startX = (float)Math.Ceiling(worldMin.X / baseStep) * baseStep;
                float lastLabelRight = float.NegativeInfinity;
                for (float x = startX; x <= worldMax.X + 1e-6f; x += baseStep)
                {
                    var px = dataToPixel(new Vector2(x, axisY));
                    var t1 = new Vector2(px.X, axisYPx - TICK_LENGTH * 0.5f);
                    var t2 = new Vector2(px.X, axisYPx + TICK_LENGTH * 0.5f);
                    ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                    var label = FormatNumberAbbrev(x);
                    int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                    float lx = px.X - tw * 0.5f;
                    float labelLeftWithMargin = lx - LABEL_MARGIN;
                    if (labelLeftWithMargin > lastLabelRight)
                    {
                        float ly = axisYPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                        ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                        lastLabelRight = lx + tw + LABEL_MARGIN;
                    }
                }
            }
        }
        else
        {
            int vTicks = Math.Max(1, ApproxTickCount);
            float lastLabelRight = float.NegativeInfinity;
            for (int i = 0; i <= vTicks; i++)
            {
                float t = i / (float)vTicks;
                var dataX = worldMin.X + t * (worldMax.X - worldMin.X);
                var px = dataToPixel(new Vector2(dataX, axisY));
                var t1 = new Vector2(px.X, axisYPx - TICK_LENGTH * 0.5f);
                var t2 = new Vector2(px.X, axisYPx + TICK_LENGTH * 0.5f);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = FormatNumberAbbrev(dataX);
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx = px.X - tw * 0.5f;
                float labelLeftWithMargin = lx - LABEL_MARGIN;
                if (labelLeftWithMargin > lastLabelRight)
                {
                    float ly = axisYPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                    ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                    lastLabelRight = lx + tw + LABEL_MARGIN;
                }
            }
        }

        // Y ticks & labels (along the Y axis)
        float axisXPx = c.X; // X pixel coordinate of the Y axis
        bool axisOnLeft = Math.Abs(axisX - worldMin.X) < 0.00001f;
        if (YTickInterval > 0f)
        {
            float baseStep = YTickInterval;
            int[] multipliers = new[] { 1, 2, 5, 10, 20, 50, 100 };
            bool placed = false;

            foreach (int mult in multipliers)
            {
                float step = baseStep * mult;
                float first = (float)Math.Ceiling(worldMin.Y / step) * step;

                // collect ticks (value + pixel Y), then sort by pixel Y (top -> bottom)
                var candidateTicks = new List<(float value, float pixelY)>();
                for (float y = first; y <= worldMax.Y + 1e-6f; y += step)
                {
                    var py = dataToPixel(new Vector2(axisX, y));
                    candidateTicks.Add((y, py.Y));
                }
                candidateTicks.Sort((a, b) => a.pixelY.CompareTo(b.pixelY));

                float lastLabelBottom = float.NegativeInfinity;
                bool collision = false;
                foreach (var item in candidateTicks)
                {
                    float pyY = item.pixelY;
                    var label = FormatNumberAbbrev(item.value);
                    int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                    float labelTop = pyY - (GRAPH_TEXT_SIZE / 2f) - LABEL_MARGIN;
                    if (labelTop <= lastLabelBottom)
                    {
                        collision = true;
                        break;
                    }
                    lastLabelBottom = pyY + (GRAPH_TEXT_SIZE / 2f) + LABEL_MARGIN;
                }

                if (!collision)
                {
                    // Draw using this multiplier (use same ordering so labels don't overlap)
                    foreach (var item in candidateTicks)
                    {
                        float y = item.value;
                        float pyY = item.pixelY;
                        var t1 = new Vector2(axisXPx - TICK_LENGTH * 0.5f, pyY);
                        var t2 = new Vector2(axisXPx + TICK_LENGTH * 0.5f, pyY);
                        ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                        var label = FormatNumberAbbrev(y);
                        int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                        float lx;
                        if (axisOnLeft)
                            lx = axisXPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                        else
                            lx = axisXPx - (TICK_LENGTH * 0.5f) - LABEL_MARGIN - tw;
                        float ly = pyY - (GRAPH_TEXT_SIZE / 2f);
                        ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                    }

                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                // fallback: greedy placement on baseStep, but sort by pixel Y to handle screen coords correctly
                float startY = (float)Math.Ceiling(worldMin.Y / baseStep) * baseStep;
                var fallbackTicks = new List<(float value, float pixelY)>();
                for (float y = startY; y <= worldMax.Y + 1e-6f; y += baseStep)
                {
                    var py = dataToPixel(new Vector2(axisX, y));
                    fallbackTicks.Add((y, py.Y));
                }
                fallbackTicks.Sort((a, b) => a.pixelY.CompareTo(b.pixelY));

                float lastLabelBottom = float.NegativeInfinity;
                foreach (var item in fallbackTicks)
                {
                    float y = item.value;
                    float pyY = item.pixelY;
                    var t1 = new Vector2(axisXPx - TICK_LENGTH * 0.5f, pyY);
                    var t2 = new Vector2(axisXPx + TICK_LENGTH * 0.5f, pyY);
                    ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                    var label = FormatNumberAbbrev(y);
                    int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                    float lx;
                    if (axisOnLeft)
                        lx = axisXPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                    else
                        lx = axisXPx - (TICK_LENGTH * 0.5f) - LABEL_MARGIN - tw;
                    float labelTop = pyY - (GRAPH_TEXT_SIZE / 2f) - LABEL_MARGIN;
                    if (labelTop > lastLabelBottom)
                    {
                        float ly = pyY - (GRAPH_TEXT_SIZE / 2f);
                        ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                        lastLabelBottom = pyY + (GRAPH_TEXT_SIZE / 2f) + LABEL_MARGIN;
                    }
                }
            }
        }
        else
        {
            int hTicks = Math.Max(1, ApproxTickCount);
            // build tick list and sort by pixel Y to avoid screen-order issues
            var autoTicks = new List<(float value, float pixelY)>();
            for (int i = 0; i <= hTicks; i++)
            {
                float t = i / (float)hTicks;
                var dataY = worldMin.Y + t * (worldMax.Y - worldMin.Y);
                var py = dataToPixel(new Vector2(axisX, dataY));
                autoTicks.Add((dataY, py.Y));
            }
            autoTicks.Sort((a, b) => a.pixelY.CompareTo(b.pixelY));

            float lastLabelBottom = float.NegativeInfinity;
            foreach (var item in autoTicks)
            {
                float dataY = item.value;
                float pyY = item.pixelY;
                var t1 = new Vector2(axisXPx - TICK_LENGTH * 0.5f, pyY);
                var t2 = new Vector2(axisXPx + TICK_LENGTH * 0.5f, pyY);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = FormatNumberAbbrev(dataY);
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx;
                if (axisOnLeft)
                    lx = axisXPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                else
                    lx = axisXPx - (TICK_LENGTH * 0.5f) - LABEL_MARGIN - tw;
                float labelTop = pyY - (GRAPH_TEXT_SIZE / 2f) - LABEL_MARGIN;
                if (labelTop > lastLabelBottom)
                {
                    float ly = pyY - (GRAPH_TEXT_SIZE / 2f);
                    ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
                    lastLabelBottom = pyY + (GRAPH_TEXT_SIZE / 2f) + LABEL_MARGIN;
                }
            }
        }

        // --- Axis Labels ---
        // X axis label: centered under the plot area
        if (!string.IsNullOrEmpty(XAxisLabel))
        {
            int tw = ray.MeasureText(XAxisLabel, GRAPH_TEXT_SIZE);
            float lx = plotRect.X + plotRect.Width * 0.5f - tw * 0.5f;

            // place label above the legend: align to reserved area (same calculation as bottomReserved)
            float labelY = bounds.Y + bounds.Height - LEGEND_HEIGHT - LABEL_MARGIN - GRAPH_TEXT_SIZE - LABEL_MARGIN;
            // fallback: if calculated label would overlap the plot bottom, clamp to just below plotRect
            if (labelY < plotRect.Y + plotRect.Height + LABEL_MARGIN)
                labelY = plotRect.Y + plotRect.Height + LABEL_MARGIN;

            ray.DrawText(XAxisLabel, (int)lx, (int)labelY, GRAPH_TEXT_SIZE, Style.TextSmall);
        }

        // Y axis label: draw vertically on the left side (stacked characters)
        if (!string.IsNullOrEmpty(YAxisLabel))
        {
            int tw = ray.MeasureText(YAxisLabel, GRAPH_TEXT_SIZE);
            float lx = plotRect.X - LABEL_MARGIN;
            float ly = plotRect.Y + plotRect.Height * 0.5f - tw * 0.5f;
            ray.DrawTextPro(ForgeWardenEngine.DefaultFont, YAxisLabel, new Vector2(lx, ly), new Vector2(), 90, GRAPH_TEXT_SIZE, 2, Style.TextSmall);
        }
    }

    private void DrawLegend(Rectangle bounds)
    {
        float x = bounds.X + 8;
        // place legend inside the bottom reserved area (above the window bottom by LABEL_MARGIN)
        float y = bounds.Y + bounds.Height - LEGEND_HEIGHT - LABEL_MARGIN;
        float slotW = 120f;
        for (int i = 0; i < series.Count; i++)
        {
            var s = series[i];
            var slotRect = new Rectangle(x + i * slotW, y, slotW - 6, LEGEND_HEIGHT - 6);
            ray.DrawRectangleRec(slotRect, Style.PanelBackground);
            ray.DrawRectangleLinesEx(slotRect, 1f, Style.PanelBorder);
            ray.DrawRectanglePro(new Rectangle(slotRect.X + 4, slotRect.Y + 6, 12, 12), new Vector2(), 0, s.Color);
            ray.DrawText(s.Name, (int)slotRect.X + 22, (int)slotRect.Y + 3, GRAPH_TEXT_SIZE, s.Color);
        }
    }

    private void HandleInput(Rectangle plotRect, Func<Vector2, Vector2> dataToPixel, Func<Vector2, Vector2> pixelToData)
    {
        if (Input.IsPressed(MouseButton.Left) && Input.IsMouseHovering(plotRect))
        {
            var mp = Input.MousePosition;
            // Check points (lines)
            for (int si = 0; si < series.Count; si++)
            {
                var s = series[si];
                if (!s.Visible) continue;

                if (s.Type == SeriesType.Line)
                {
                    for (int pi = 0; pi < s.Points.Count; pi++)
                    {
                        var ppx = dataToPixel(s.Points[pi]);
                        if (Vector2Distance(ppx, mp) <= POINT_HIT_RADIUS)
                        {
                            selectedPoint = (si, pi);
                            return;
                        }
                    }
                }
                else if (s.Type == SeriesType.Bar)
                {
                    for (int pi = 0; pi < s.Points.Count; pi++)
                    {
                        var centerPx = dataToPixel(new Vector2(s.Points[pi].X, 0)).X;
                        float halfW = s.BarWidth * 0.5f;
                        var topPx = dataToPixel(new Vector2(s.Points[pi].X, s.Points[pi].Y)).Y;
                        var basePx = dataToPixel(new Vector2(s.Points[pi].X, 0)).Y;
                        var rect = new Rectangle(centerPx - halfW, Math.Min(topPx, basePx), halfW * 2, Math.Abs(basePx - topPx));
                        if (ray.CheckCollisionPointRec(mp, rect))
                        {
                            selectedPoint = (si, pi);
                            return;
                        }
                    }
                }
            }

            // If none hit, clear selection
            selectedPoint = null;
        }
    }

    private static float Vector2Distance(Vector2 a, Vector2 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public Series GetSeries(string name, Color? color = null)
    {
        Series s = series.FirstOrDefault(s => s.Name == name);
        if (s == null)
        {
            s = new Series(name, MaxDataPoints, SeriesType.Line);
            s.Color = color ?? Color.White;
            series.Add(s);
        }
        return s;
    }

    public void SetSeriesColor(string name, Color c)
    {
        Series s = series.FirstOrDefault(s => s.Name == name);
        if (s == null)
        {
            s = new Series(name, MaxDataPoints, SeriesType.Line);
            series.Add(s);
        }
        s.Color = c;
    }

    public IReadOnlyList<Series> GetSeries() => series;

    private float GetLowestYTick(Vector2 worldMin, Vector2 worldMax)
    {
        if (YTickInterval > 0f)
        {
            return (float)Math.Ceiling(worldMin.Y / YTickInterval) * YTickInterval;
        }
        return worldMin.Y;
    }
}

