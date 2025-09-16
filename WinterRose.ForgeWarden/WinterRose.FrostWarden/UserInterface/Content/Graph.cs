using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface;
public class Graph : UIContent
{
    private const float AXIS_MARGIN = 32f;
    private const float LEGEND_HEIGHT = 24f;
    private const float POINT_HIT_RADIUS = 6f;
    private const float DEFAULT_BAR_WIDTH = 12f;
    private const float ZOOM_STEP = 1.1f;
    private const float MIN_ZOOM = 0.1f;
    private const float MAX_ZOOM = 10f;
    private const int GRAPH_TEXT_SIZE = 12;

    private const float TICK_LENGTH = 8f;
    private const float LABEL_MARGIN = 4f;

    private readonly List<Series> series = new();
    private Vector2 pan = new(0, 0);
    private bool isPanning;
    private Vector2 panStart;
    private (int seriesIndex, int pointIndex)? selectedPoint = null;

    /// <summary>Tick interval along X in data units. Set >0 to force a fixed interval. Set to 0 for automatic spacing.</summary>
    public float XTickInterval { get; set; } = 0f;

    /// <summary>Tick interval along Y in data units. Set >0 to force a fixed interval. Set to 0 for automatic spacing.</summary>
    public float YTickInterval { get; set; } = 0f;

    /// <summary>Used when interval is automatic: approximate number of ticks.</summary>
    public int ApproxTickCount { get; set; } = DEFAULT_TICK_COUNT;

    public string XAxisLabel { get; set; } = "";
    public string YAxisLabel { get; set; } = "";

    private const int DEFAULT_TICK_COUNT = 5;

    public class Series
    {
        public List<Vector2> Points { get; } = new();
        public Color Color { get; set; } = Color.White;
        public bool Visible { get; set; } = true;
        public float BarWidth { get; set; } = DEFAULT_BAR_WIDTH;
        public string Name { get; set; }
        public SeriesType Type { get; set; }

        public Series(string Name, SeriesType Type)
        {
            this.Name = Name;
            this.Type = Type;
        }

        public void AddPoint(float x, float y)
        {
            Points.Add(new Vector2(x, y));
        }
    }

    public enum SeriesType
    {
        Line,
        Bar
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // Request to take all available area by default
        return new Vector2(availableArea.Width, availableArea.Height);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        // Default aspect: width : height = 2 : 1
        return maxWidth * 0.5f + LEGEND_HEIGHT + GRAPH_TEXT_SIZE + LABEL_MARGIN * 6;
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
            bounds.Y + 8,
            bounds.Width - AXIS_MARGIN - 8,
            bounds.Height - AXIS_MARGIN / 2 - bottomReserved);

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

        // Convert data -> pixel
        Vector2 DataToPixel(Vector2 data)
        {
            float dataWidth = (worldMax.X - worldMin.X);
            float dataHeight = (worldMax.Y - worldMin.Y);

            // normalized within data bounds [0..1]
            float nx = (data.X - worldMin.X) / dataWidth;
            float ny = (data.Y - worldMin.Y) / dataHeight;

            // apply zoom and pan around center
            var center = new Vector2(plotRect.X + plotRect.Width / 2, plotRect.Y + plotRect.Height / 2);
            var scaledWidth = plotRect.Width;
            var scaledHeight = plotRect.Height;

            var px = center.X - (scaledWidth / 2) + nx * scaledWidth + pan.X;
            var py = center.Y + (scaledHeight / 2) - ny * scaledHeight + pan.Y; // y flipped

            return new Vector2(px, py);
        }

        Vector2 PixelToData(Vector2 pixel)
        {
            var center = new Vector2(plotRect.X + plotRect.Width / 2, plotRect.Y + plotRect.Height / 2);
            var scaledWidth = plotRect.Width;
            var scaledHeight = plotRect.Height;

            float nx = (pixel.X - (center.X - scaledWidth / 2) - pan.X) / scaledWidth;
            float ny = (center.Y + scaledHeight / 2 - pixel.Y - pan.Y) / scaledHeight;

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
                // Bars are drawn around x coordinate; baseline at y=0 (or min world)
                float baseY = 0f;
                if (worldMin.Y > 0 && worldMax.Y > 0)
                    baseY = worldMin.Y;
                else if (worldMin.Y < 0 && worldMax.Y < 0)
                    baseY = worldMax.Y;
                var basePixel = DataToPixel(new Vector2(0, baseY));

                for (int p = 0; p < s.Points.Count; p++)
                {
                    var dataPoint = s.Points[p];
                    var centerPx = DataToPixel(new Vector2(dataPoint.X, 0)).X; // center X position for this x
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
                var tipRect = new Rectangle(pPx.X + 8, pPx.Y - 16, tw + 8, 20);
                ray.DrawRectangleRec(tipRect, Style.TooltipBackground);
                ray.DrawText(tip, (int)tipRect.X + 4, (int)tipRect.Y + 3, GRAPH_TEXT_SIZE, Style.TextSmall);
            }
        }

        // Axes (draw on top)
        DrawAxes(plotRect, worldMin, worldMax, bounds, DataToPixel);

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
        var s = new Series(name, SeriesType.Line) { Color = color ?? Color.White };
        s.Points.AddRange(points);
        series.Add(s);
    }

    public void AddBarSeries(string name, IEnumerable<Vector2> points, Color? color = null, float barWidth = DEFAULT_BAR_WIDTH)
    {
        var s = new Series(name, SeriesType.Bar) { Color = color ?? Color.White, BarWidth = barWidth };
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

    // updated method: DrawAxes (full replacement)
    private void DrawAxes(Rectangle plotRect, Vector2 worldMin, Vector2 worldMax, Rectangle bounds, Func<Vector2, Vector2> dataToPixel)
    {
        // X axis at y = 0 if visible, else bottom
        float axisY = worldMin.Y <= 0 && worldMax.Y >= 0 ? 0 : worldMin.Y;
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
            float startX = (float)Math.Ceiling(worldMin.X / XTickInterval) * XTickInterval;
            for (float x = startX; x <= worldMax.X + 1e-6f; x += XTickInterval)
            {
                var px = dataToPixel(new Vector2(x, axisY));
                var t1 = new Vector2(px.X, axisYPx - TICK_LENGTH * 0.5f);
                var t2 = new Vector2(px.X, axisYPx + TICK_LENGTH * 0.5f);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = x.ToString("0.###");
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx = px.X - tw * 0.5f;
                float ly = axisYPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
            }
        }
        else
        {
            int vTicks = Math.Max(1, ApproxTickCount);
            for (int i = 0; i <= vTicks; i++)
            {
                float t = i / (float)vTicks;
                var dataX = worldMin.X + t * (worldMax.X - worldMin.X);
                var px = dataToPixel(new Vector2(dataX, axisY));
                var t1 = new Vector2(px.X, axisYPx - TICK_LENGTH * 0.5f);
                var t2 = new Vector2(px.X, axisYPx + TICK_LENGTH * 0.5f);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = dataX.ToString("0.###");
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx = px.X - tw * 0.5f;
                float ly = axisYPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
            }
        }

        // Y ticks & labels (along the Y axis)
        float axisXPx = c.X; // X pixel coordinate of the Y axis
        bool axisOnLeft = Math.Abs(axisX - worldMin.X) < 0.00001f;
        if (YTickInterval > 0f)
        {
            float startY = (float)Math.Ceiling(worldMin.Y / YTickInterval) * YTickInterval;
            for (float y = startY; y <= worldMax.Y + 1e-6f; y += YTickInterval)
            {
                var py = dataToPixel(new Vector2(axisX, y));
                var t1 = new Vector2(axisXPx - TICK_LENGTH * 0.5f, py.Y);
                var t2 = new Vector2(axisXPx + TICK_LENGTH * 0.5f, py.Y);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = y.ToString("0.###");
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx;
                if (axisOnLeft)
                    lx = axisXPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                else
                    lx = axisXPx - (TICK_LENGTH * 0.5f) - LABEL_MARGIN - tw;
                float ly = py.Y - (GRAPH_TEXT_SIZE / 2f);
                ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
            }
        }
        else
        {
            int hTicks = Math.Max(1, ApproxTickCount);
            for (int i = 0; i <= hTicks; i++)
            {
                float t = i / (float)hTicks;
                var dataY = worldMin.Y + t * (worldMax.Y - worldMin.Y);
                var py = dataToPixel(new Vector2(axisX, dataY));
                var t1 = new Vector2(axisXPx - TICK_LENGTH * 0.5f, py.Y);
                var t2 = new Vector2(axisXPx + TICK_LENGTH * 0.5f, py.Y);
                ray.DrawLineEx(t1, t2, 1f, Style.AxisLine);

                var label = dataY.ToString("0.###");
                int tw = ray.MeasureText(label, GRAPH_TEXT_SIZE);
                float lx;
                if (axisOnLeft)
                    lx = axisXPx + (TICK_LENGTH * 0.5f) + LABEL_MARGIN;
                else
                    lx = axisXPx - (TICK_LENGTH * 0.5f) - LABEL_MARGIN - tw;
                float ly = py.Y - (GRAPH_TEXT_SIZE / 2f);
                ray.DrawText(label, (int)lx, (int)ly, GRAPH_TEXT_SIZE, Style.TextSmall);
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
            ray.DrawTextPro(ray.GetFontDefault(), YAxisLabel, new Vector2(lx, ly), new Vector2(), 90, GRAPH_TEXT_SIZE, 2, Style.TextSmall);
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

    public Series GetSeries(string name)
    {
        Series s = series.FirstOrDefault(s => s.Name == name);
        if(s == null)
        {
            s = new Series(name, SeriesType.Line);
            series.Add(s);
        }
        return s;
    }

    public IReadOnlyList<Series> GetSeries() => series;
}

