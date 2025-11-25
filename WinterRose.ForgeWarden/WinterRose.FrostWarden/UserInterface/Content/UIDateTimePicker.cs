using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WinterRose.ForgeSignal;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIDateTimePicker : UIContent
{
    public enum PickerMode { DateOnly, TimeOnly, DateTime }

    private enum CalendarZoomLevel { Month, Year, Decade }
    private CalendarZoomLevel calendarZoom = CalendarZoomLevel.Month;
    private CalendarZoomLevel CalendarZoom
    {
        get => calendarZoom;
        set
        {
            calendarZoom = value;
            decadeStartYear = (visibleMonth.Year / 12) * 12;
        }
    }
    private int decadeStartYear { get; set; } // first year of current decade page


    // Public API
    public PickerMode Mode { get; set; } = PickerMode.DateTime;
    public bool Use24Hour { get; set; } = false; // otherwise 12h with AM/PM toggle
    public DateTime Selected
    {
        get => selected;
        set
        {
            selected = ClampToRange(value);
            SyncInternalFromSelected();
            OnDateTimeChanged?.Invoke(owner, this, selected);
            OnDateTimeChangedBasic?.Invoke(selected);
        }
    }

    public MulticastVoidInvocation<UIContainer, UIDateTimePicker, DateTime> OnDateTimeChanged { get; set; } = new();
    public MulticastVoidInvocation<DateTime> OnDateTimeChangedBasic { get; set; } = new();

    // Layout constants
    private const float CONTROL_HEIGHT = 320f;
    private const float PADDING = 8f;
    private const float MONTH_HEADER_HEIGHT = 28f;
    private const float WEEKDAY_HEIGHT = 20f;
    private const float CELL_PADDING = 6f;
    private const float CLOCK_RADIUS_RATIO = 0.38f; // fraction of width used for clock radius

    // Internal state
    private DateTime selected = DateTime.Now;
    private DateTime visibleMonth = DateTime.Now; // month shown in calendar (1st of month)
    private List<Rectangle> dayCellRects = new();
    private Rectangle monthHeaderRect = new Rectangle();
    private Rectangle calendarRect = new Rectangle();
    private Rectangle timePanelRect = new Rectangle();
    private Rectangle wholeBounds = new Rectangle();

    // Time selection internal
    private bool selectingHours = true; // true => picking hours, false => picking minutes
    private int hoveredHour = -1;
    private int hoveredMinute = -1;

    // Interaction helpers
    private bool isDraggingClock = false;
    private bool isHovering = false;

    private float innerHourRadius;
    private float outerHourRadius;
    private Rectangle toggleTimeModeButton;
    private Rectangle prevMonthRect;
    private Rectangle nextMonthRect;
    private Rectangle zoomOutRect;

    // ctor
    public UIDateTimePicker()
    {
        // keep selected in legal range
        selected = ClampToRange(selected);
        visibleMonth = new DateTime(selected.Year, selected.Month, 1);
        SyncInternalFromSelected();
    }

    // keep the displayed month consistent with selected date
    private void SyncInternalFromSelected()
    {
        if (selected < DateTime.MinValue) selected = DateTime.MinValue;
        if (selected > DateTime.MaxValue) selected = DateTime.MaxValue;
        visibleMonth = new DateTime(selected.Year, selected.Month, 1);
    }

    private DateTime ClampToRange(DateTime dt)
    {
        if (dt < DateTime.MinValue) return DateTime.MinValue;
        if (dt > DateTime.MaxValue) return DateTime.MaxValue;
        return dt;
    }

    // UI lifecycle ----------------------------------------------------------

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // width uses full available width; height constant
        return new Vector2(availableArea.Width, CONTROL_HEIGHT);
    }

    protected internal override float GetHeight(float maxWidth) => CONTROL_HEIGHT;

    protected internal override void Setup()
    {
        // nothing special
    }

    protected internal override void Update()
    {
        // hover detection for the overall control (used for subtle interactions)
        var mp = Input.Provider.MousePosition;
        bool oldHover = isHovering;
        isHovering = mp.X >= wholeBounds.X && mp.X <= wholeBounds.X + wholeBounds.Width &&
                     mp.Y >= wholeBounds.Y && mp.Y <= wholeBounds.Y + wholeBounds.Height;
        if (isHovering && !oldHover) OnHover();
        else if (!isHovering && oldHover) OnHoverEnd();

        // If dragging on clock, update the selection
        if (isDraggingClock)
        {
            var m = Input.Provider.MousePosition;
            UpdateClockSelectionFromPoint(m.X, m.Y, true);
            if (!Input.IsDown(MouseButton.Left))
                isDraggingClock = false;
        }
    }

    // helper: month navigation
    private void MoveMonth(int delta)
    {
        // move visibleMonth by delta months respecting DateTime bounds
        var y = visibleMonth.Year;
        var m = visibleMonth.Month + delta;
        y += (m - 1) / 12;
        m = ((m - 1) % 12 + 12) % 12 + 1;
        int day = Math.Min(visibleMonth.Day, DateTime.DaysInMonth(y, m));
        try
        {
            visibleMonth = new DateTime(y, m, 1);
        }
        catch
        {
            // if out of bounds, clamp to min/max
            if (y < DateTime.MinValue.Year) visibleMonth = new DateTime(DateTime.MinValue.Year, 1, 1);
            else visibleMonth = new DateTime(DateTime.MaxValue.Year, 12, 1);
        }
    }

    private void MoveYear(int delta)
    {
        int y = visibleMonth.Year + delta;
        int m = visibleMonth.Month;
        y = Math.Clamp(y, DateTime.MinValue.Year, DateTime.MaxValue.Year);
        visibleMonth = new DateTime(y, m, 1);
    }

    // calendar helpers
    private static int DayOfWeekIndex(DayOfWeek d) => ((int)d + 7) % 7; // 0=Sun .. 6=Sat

    // compute day grid and rectangles based on calendarRect
    private void BuildCalendarGrid(Rectangle bounds)
    {
        dayCellRects.Clear();

        // 7 columns, number of rows depends (5 or 6)
        int firstDayWeek = DayOfWeekIndex(new DateTime(visibleMonth.Year, visibleMonth.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(visibleMonth.Year, visibleMonth.Month);
        int cellsBefore = firstDayWeek;
        int totalCells = cellsBefore + daysInMonth;
        int rows = (int)Math.Ceiling(totalCells / 7.0);
        if (rows < 5) rows = 5; // keep consistent shape
        float cellW = bounds.Width / 7f;
        float cellH = (bounds.Height - WEEKDAY_HEIGHT) / (float)rows;

        // store rectangles row-major skipping weekday header space
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < 7; c++)
            {
                float x = bounds.X + c * cellW;
                float y = bounds.Y + WEEKDAY_HEIGHT + r * cellH;
                var rc = new Rectangle((int)x, (int)y, (int)Math.Ceiling(cellW), (int)Math.Ceiling(cellH));
                dayCellRects.Add(rc);
            }
        }
    }

    // drawing ---------------------------------------------------------------

    protected override void Draw(Rectangle bounds)
    {
        wholeBounds = bounds;

        // Background panel
        ray.DrawRectangleRec(bounds, Style.TextBoxBackground);
        ray.DrawRectangleLinesEx(bounds, 1f, Style.TextBoxBorder);

        // split area: left = calendar (2/3), right = time panel (1/3) when mode includes time.
        bool showCalendar = Mode == PickerMode.DateOnly || Mode == PickerMode.DateTime;
        bool showTime = Mode == PickerMode.TimeOnly || Mode == PickerMode.DateTime;

        float leftW = showCalendar ? bounds.Width * (showTime ? 0.66f : 0.98f) : 0;
        float rightW = showTime ? bounds.Width - leftW - PADDING : 0;
        Rectangle leftRect = new Rectangle(bounds.X + PADDING, bounds.Y + PADDING, (int)(leftW - PADDING), (int)(bounds.Height - PADDING * 2));
        Rectangle rightRect = showTime ? new Rectangle((int)(bounds.X + leftW + PADDING), bounds.Y + PADDING, (int)(rightW - PADDING), (int)(bounds.Height - PADDING * 2)) : new Rectangle();

        if (showCalendar)
        {
            calendarRect = leftRect;
            DrawCalendar(leftRect);
        }
        else
        {
            calendarRect = new Rectangle(); 
        }

        if (showTime)
        {
            timePanelRect = rightRect; 
            DrawTimePicker(rightRect);
        }
        else
        {
            timePanelRect = new Rectangle(); 
        }

        ray.DrawRectanglePro(prevMonthRect, new(), 0, Color.Magenta);
        ray.DrawRectanglePro(nextMonthRect, new(), 0, Color.Magenta);
    }

    private void DrawCalendar(Rectangle area)
    {
        var font = Raylib.GetFontDefault();
        float fs = Style.BaseButtonFontSize;
        float spacing = Style.BaseButtonFontSize * 0.1f;

        // header: always show current month/year or year range
        monthHeaderRect = new Rectangle(area.X, area.Y, area.Width, MONTH_HEADER_HEIGHT);
        ray.DrawRectangleRec(monthHeaderRect, Style.ButtonBackground);
        ray.DrawRectangleLinesEx(monthHeaderRect, 1f, Style.ButtonBorder);

        string headerText = CalendarZoom switch
        {
            CalendarZoomLevel.Month => $"{visibleMonth.ToString("MMMM", CultureInfo.InvariantCulture)} {visibleMonth.Year}",
            CalendarZoomLevel.Year => $"{visibleMonth.Year}",
            CalendarZoomLevel.Decade => $"{decadeStartYear} - {decadeStartYear + 11}",
            _ => ""
        };
        Vector2 headerSize = Raylib.MeasureTextEx(font, headerText, fs, spacing);
        Vector2 headerPos = new Vector2(monthHeaderRect.X + (monthHeaderRect.Width - headerSize.X) / 2f,
                                        monthHeaderRect.Y + (monthHeaderRect.Height - headerSize.Y) / 2f);
        Raylib.DrawTextEx(font, headerText, headerPos, fs, spacing, Style.TextBoxText);

        // nav buttons
        int navW = 20;
        int padding = 6;
        prevMonthRect = new Rectangle(monthHeaderRect.X + padding, monthHeaderRect.Y + 4, navW, (int)(MONTH_HEADER_HEIGHT - 8));
        nextMonthRect = new Rectangle(monthHeaderRect.X + monthHeaderRect.Width - navW - padding, monthHeaderRect.Y + 4, navW, (int)(MONTH_HEADER_HEIGHT - 8));

        // Zoom-out button in the center
        int zoomW = 40;
        zoomOutRect = new Rectangle(monthHeaderRect.X + (monthHeaderRect.Width - zoomW) * 0.5f, monthHeaderRect.Y + 4, zoomW, (int)(MONTH_HEADER_HEIGHT - 8));

        // draw buttons with hover effect
        Color prevColor = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, prevMonthRect) ? Style.ButtonHover : Style.ButtonBackground;
        Color nextColor = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, nextMonthRect) ? Style.ButtonHover : Style.ButtonBackground;
        Color zoomColor = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, zoomOutRect) ? Style.ButtonHover : Style.ButtonBackground;

        ray.DrawRectangleRec(prevMonthRect, prevColor);
        ray.DrawRectangleRec(nextMonthRect, nextColor);
        ray.DrawRectangleRec(zoomOutRect, zoomColor);

        ray.DrawRectangleLinesEx(prevMonthRect, 1f, Style.ButtonBorder);
        ray.DrawRectangleLinesEx(nextMonthRect, 1f, Style.ButtonBorder);
        ray.DrawRectangleLinesEx(zoomOutRect, 1f, Style.ButtonBorder);

        // draw arrows and zoom symbol
        Raylib.DrawTextEx(font, "<", new Vector2(prevMonthRect.X + 5, prevMonthRect.Y + 3), fs, spacing, Style.TextBoxText);
        Raylib.DrawTextEx(font, ">", new Vector2(nextMonthRect.X + 5, nextMonthRect.Y + 3), fs, spacing, Style.TextBoxText);
        Raylib.DrawTextEx(font, "⤢", new Vector2(zoomOutRect.X + 10, zoomOutRect.Y + 3), fs, spacing, Style.TextBoxText); // ⤢ for zoom-out

        // draw main calendar content
        switch (CalendarZoom)
        {
            case CalendarZoomLevel.Month:
                DrawMonthDayGrid(area);
                break;
            case CalendarZoomLevel.Year:
                DrawYearOverview(area);
                break;
            case CalendarZoomLevel.Decade:
                DrawDecadeOverview(area);
                break;
        }
    }


    private void DrawMonthDayGrid(Rectangle gridArea)
    {
        var font = Raylib.GetFontDefault();
        float fs = Style.BaseButtonFontSize;
        float spacing = Style.BaseButtonFontSize * 0.1f;

        // build grid rects for days
        BuildCalendarGrid(gridArea);

        int firstWeek = DayOfWeekIndex(new DateTime(visibleMonth.Year, visibleMonth.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(visibleMonth.Year, visibleMonth.Month);

        for (int i = 0; i < dayCellRects.Count; i++)
        {
            var cell = dayCellRects[i];
            int dayIndex = i - firstWeek + 1;
            bool inMonth = dayIndex >= 1 && dayIndex <= daysInMonth;

            // determine if today
            bool isToday = false;
            try
            {
                isToday = inMonth && visibleMonth.Year == DateTime.Now.Year
                                   && visibleMonth.Month == DateTime.Now.Month
                                   && dayIndex == DateTime.Now.Day;
            }
            catch { }

            // determine if selected
            bool isSelectedDay = inMonth && selected.Year == visibleMonth.Year
                                          && selected.Month == visibleMonth.Month
                                          && selected.Day == dayIndex;

            // background color
            Color bg = new Color(0, 0, 0, 0);
            if (isSelectedDay) bg = Style.ButtonHover;
            else if (isToday) bg = new Color(200, 200, 240, 120);

            if (bg.A > 0) ray.DrawRectangleRec(cell, bg);
            ray.DrawRectangleLinesEx(cell, 1f, Style.TextBoxBorder);

            // day number
            string sDay = inMonth ? dayIndex.ToString() : "";
            Vector2 ds = Raylib.MeasureTextEx(font, sDay, fs * 0.9f, spacing);
            Vector2 dp = new Vector2(cell.X + CELL_PADDING, cell.Y + (cell.Height - ds.Y) / 2f);
            Color dayColor = inMonth ? Style.TextBoxText : new Color(150, 150, 150, (int)(255 * Style.ContentAlpha));
            Raylib.DrawTextEx(font, sDay, dp, fs * 0.9f, spacing, dayColor);
        }
    }


    private void DrawYearOverview(Rectangle area)
    {
        var font = Raylib.GetFontDefault();
        float fs = Style.BaseButtonFontSize * 0.85f;
        float spacing = Style.BaseButtonFontSize * 0.1f;

        float cellW = area.Width / 3f; // 3 columns
        float cellH = (area.Height - MONTH_HEADER_HEIGHT) / 4f; // 4 rows

        for (int i = 1; i <= 12; i++)
        {
            int monthNum = i;
            float x = area.X + (i - 1) % 3 * cellW;
            float y = area.Y + MONTH_HEADER_HEIGHT + (i - 1) / 3 * cellH;
            Rectangle cell = new Rectangle(x, y, cellW, cellH);

            Color bg = (visibleMonth.Month == monthNum && CalendarZoom == CalendarZoomLevel.Month) ? Style.ButtonHover : Style.ButtonBackground;
            ray.DrawRectangleRec(cell, bg);
            ray.DrawRectangleLinesEx(cell, 1f, Style.ButtonBorder);

            string label = new DateTime(2000, monthNum, 1).ToString("MMM", CultureInfo.InvariantCulture);
            Vector2 sz = Raylib.MeasureTextEx(font, label, fs, spacing);
            Vector2 pos = new Vector2(x + (cellW - sz.X) / 2f, y + (cellH - sz.Y) / 2f);
            Raylib.DrawTextEx(font, label, pos, fs, spacing, Style.TextBoxText);
        }
    }

    private void DrawDecadeOverview(Rectangle area)
    {
        var font = Raylib.GetFontDefault();
        float fs = Style.BaseButtonFontSize * 0.8f;
        float spacing = Style.BaseButtonFontSize * 0.1f;

        float cellW = area.Width / 3f; // 3 columns
        float cellH = (area.Height - MONTH_HEADER_HEIGHT) / 4f; // 4 rows

        for (int i = 0; i < 12; i++)
        {
            int year = decadeStartYear + i;
            float x = area.X + i % 3 * cellW;
            float y = area.Y + MONTH_HEADER_HEIGHT + i / 3 * cellH;
            Rectangle cell = new Rectangle(x, y, cellW, cellH);

            Color bg = (visibleMonth.Year == year && CalendarZoom == CalendarZoomLevel.Year) ? Style.ButtonHover : Style.ButtonBackground;
            ray.DrawRectangleRec(cell, bg);
            ray.DrawRectangleLinesEx(cell, 1f, Style.ButtonBorder);

            string label = year.ToString();
            Vector2 sz = Raylib.MeasureTextEx(font, label, fs, spacing);
            Vector2 pos = new Vector2(x + (cellW - sz.X) / 2f, y + (cellH - sz.Y) / 2f);
            Raylib.DrawTextEx(font, label, pos, fs, spacing, Style.TextBoxText);
        }
    }

    private void DrawTimePicker(Rectangle area)
    {
        timePanelRect = area;
        ray.DrawRectangleRec(area, new Color(0, 0, 0, 0)); // transparent bg
        ray.DrawRectangleLinesEx(area, 1f, Style.TextBoxBorder);

        // top shows selected time text and AM/PM toggle if necessary
        var font = Raylib.GetFontDefault();
        float fs = Style.BaseButtonFontSize;
        float spacing = Style.BaseButtonFontSize * 0.1f;

        string timeText = selected.ToString(Use24Hour ? "HH:mm" : "hh:mm tt", CultureInfo.InvariantCulture);
        Vector2 tt = Raylib.MeasureTextEx(font, timeText, fs, spacing);
        Vector2 tpos = new Vector2(area.X + (area.Width - tt.X) / 2f, area.Y + 8);
        Raylib.DrawTextEx(font, timeText, tpos, fs, spacing, Style.TextBoxText);

        toggleTimeModeButton = new Rectangle(
            tpos.X - 6, tpos.Y - 4,
            tt.X + 12, tt.Y + 8);

        string toggleLabel = selectingHours ? "Pick Minutes" : "Pick Hours";
        Vector2 lblSize = Raylib.MeasureTextEx(font, toggleLabel, fs * 0.7f, spacing);
        Vector2 lblPos = new Vector2(area.X + (area.Width - lblSize.X) / 2f, tpos.Y + tt.Y + 6);

        toggleTimeModeButton = new Rectangle(
            lblPos.X - 6, lblPos.Y - 4,
            lblSize.X + 12, lblSize.Y + 8
        );

        // draw button background + text
        bool hovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), toggleTimeModeButton);
        Color bg = hovered ? Style.ButtonHover : Style.ButtonBackground;
        ray.DrawRectangleRec(toggleTimeModeButton, bg);
        ray.DrawRectangleLinesEx(toggleTimeModeButton, 1f, Style.ButtonBorder);
        Raylib.DrawTextEx(font, toggleLabel, lblPos, fs * 0.7f, spacing, Style.TextBoxText);


        // AM/PM toggle if in 12-hour mode
        if (!Use24Hour)
        {
            Rectangle amRect = new Rectangle(area.X + 10, area.Y + 8, 48, 20);
            Rectangle pmRect = new Rectangle(area.X + area.Width - 58, area.Y + 8, 48, 20);
            bool amHover = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, amRect);
            bool pmHover = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, pmRect);

            ray.DrawRectangleRec(amRect, (selected.Hour < 12) ? Style.ButtonHover : Style.ButtonBackground);
            ray.DrawRectangleLinesEx(amRect, 1f, Style.ButtonBorder);
            ray.DrawRectangleRec(pmRect, (selected.Hour >= 12) ? Style.ButtonHover : Style.ButtonBackground);
            ray.DrawRectangleLinesEx(pmRect, 1f, Style.ButtonBorder);

            Raylib.DrawTextEx(font, "AM", new Vector2(amRect.X + 6, amRect.Y + 2), fs * 0.8f, spacing, Style.TextBoxText);
            Raylib.DrawTextEx(font, "PM", new Vector2(pmRect.X + 6, pmRect.Y + 2), fs * 0.8f, spacing, Style.TextBoxText);
            // clicks on these handled in OnContentClicked
        }

        // clock area: centered circle
        float cx = area.X + area.Width * 0.5f;
        float cy = area.Y + area.Height * 0.5f + 8;
        float radius = Math.Min(area.Width, area.Height) * CLOCK_RADIUS_RATIO;

        innerHourRadius = radius * 0.55f;  // inner ring radius (1–12 in 24h mode)
        outerHourRadius = radius * 0.85f;  // outer ring radius (13–24 in 24h mode)

        Raylib.DrawCircle((int)cx, (int)cy, radius, Style.TextBoxBackground);
        Raylib.DrawCircleLines((int)cx, (int)cy, radius, Style.TextBoxBorder);

        if (selectingHours)
        {
            if (Use24Hour)
            {
                // 24 divisions (0..23)
                for (int h = 0; h < 24; h++)
                {
                    bool inner = (h >= 0 && h < 12);  // 0–11 inner ring
                    double ang = h * Math.PI * 2.0 / 12.0 - Math.PI / 2.0; // 12 numbers per ring
                    float ringR = inner ? radius * 0.55f : radius * 0.75f; // inner smaller, outer bigger

                    float tx = cx + (float)Math.Cos(ang) * ringR;
                    float ty = cy + (float)Math.Sin(ang) * ringR;
                    string hs = h.ToString();
                    Vector2 hsiz = Raylib.MeasureTextEx(Raylib.GetFontDefault(), hs, fs * 0.75f, spacing);
                    Vector2 hpos = new Vector2(tx - hsiz.X / 2f, ty - hsiz.Y / 2f);

                    bool isSelHour = selectingHours && (selected.Hour == h);
                    Color hourColor = isSelHour ? Style.ButtonHover : Style.TextBoxText;
                    Raylib.DrawTextEx(Raylib.GetFontDefault(), hs, hpos, fs * 0.75f, spacing, hourColor);
                }
            }
            else
            {
                // 12-hour face (1..12)
                for (int h = 1; h <= 12; h++)
                {
                    double ang = (h % 12) * Math.PI * 2.0 / 12.0 - Math.PI / 2.0; // top=12
                    float tx = cx + (float)Math.Cos(ang) * (radius * 0.70f);
                    float ty = cy + (float)Math.Sin(ang) * (radius * 0.70f);
                    string hs = h.ToString();
                    Vector2 hsiz = Raylib.MeasureTextEx(Raylib.GetFontDefault(), hs, fs * 0.9f, spacing);
                    Vector2 hpos = new Vector2(tx - hsiz.X / 2f, ty - hsiz.Y / 2f);

                    int curHour12 = ((selected.Hour % 12) == 0) ? 12 : (selected.Hour % 12);
                    bool isSelHour = (curHour12 == h) && selectingHours;
                    Color hourColor = isSelHour ? Style.ButtonHover : Style.TextBoxText;

                    Raylib.DrawTextEx(Raylib.GetFontDefault(), hs, hpos, fs * 0.9f, spacing, hourColor);
                }
            }
        }
        else
        {
            // Draw 60 minute ticks
            for (int m = 0; m < 60; m += 5) // every 5 min
            {
                double ang = m * Math.PI * 2.0 / 60.0 - Math.PI / 2.0;
                float tx = cx + (float)Math.Cos(ang) * (radius * 0.75f);
                float ty = cy + (float)Math.Sin(ang) * (radius * 0.75f);
                string ms = m.ToString("D2");
                Vector2 msiz = Raylib.MeasureTextEx(Raylib.GetFontDefault(), ms, fs * 0.65f, spacing);
                Vector2 mpos = new Vector2(tx - msiz.X / 2f, ty - msiz.Y / 2f);

                bool isSelMin = !selectingHours && (selected.Minute == m);
                Color minColor = isSelMin ? Style.ButtonHover : Style.TextBoxText;
                Raylib.DrawTextEx(Raylib.GetFontDefault(), ms, mpos, fs * 0.65f, spacing, minColor);
            }
        }

        // draw small center dot and hand showing current animated selection
        // compute hand end using current hover if dragging, otherwise selected hour/minute
        double displayHour = selected.Hour % 12;
        double displayMinute = selected.Minute;
        if (selectingHours && hoveredHour > 0) displayHour = hoveredHour % 12;
        if (!selectingHours && hoveredMinute >= 0) displayMinute = hoveredMinute;

        double handAngle;
        if (selectingHours)
        {
            if (Use24Hour)
            {
                handAngle = ((displayHour) * Math.PI * 2.0 / 12.0 - Math.PI / 2.0);
            }
            else
            {
                handAngle = ((displayHour % 12) * Math.PI * 2.0 / 12.0 - Math.PI / 2.0);
            }
        }
        else
        {
            handAngle = (displayMinute * Math.PI * 2.0 / 60.0 - Math.PI / 2.0);
        }
        float handLen = selectingHours ? (Use24Hour ? radius * 0.55f : radius * 0.55f) : radius * 0.85f;
        Vector2 handEnd = new Vector2(cx + (float)Math.Cos(handAngle) * handLen, cy + (float)Math.Sin(handAngle) * handLen);
        Raylib.DrawLineEx(new Vector2(cx, cy), handEnd, 3f, Style.ButtonHover);
        Raylib.DrawCircle((int)cx, (int)cy, 4f, Style.ButtonBorder);
    }

    // click handling -------------------------------------------------------

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button != MouseButton.Left) return;

        var m = Input.Provider.MousePosition;

        // toggle button
        if (Raylib.CheckCollisionPointRec(m, toggleTimeModeButton))
        {
            selectingHours = !selectingHours;
            return; // prevent immediate time selection
        }

        // Calendar interactions
        if (!calendarRect.Equals(new Rectangle()) && Raylib.CheckCollisionPointRec(m, calendarRect))
        {
            HandleCalendarHeaderClick(m); // header nav: prev/next month/year
            HandleCalendarCellClick(m); 
            return;
        }

        // Time interactions
        if (!calendarRect.Equals(new Rectangle()) && Raylib.CheckCollisionPointRec(m, timePanelRect))
        {
            HandleAmPmClick(m);
            HandleClockFaceClick(m);
        }
    }

    private void HandleCalendarHeaderClick(Vector2 mousePos)
    {
        // Left/right arrows always move the view depending on zoom level
        int navW = 20;
        int headerH = (int)MONTH_HEADER_HEIGHT;


        if (Raylib.CheckCollisionPointRec(mousePos, prevMonthRect))
        {
            switch (CalendarZoom)
            {
                case CalendarZoomLevel.Month: MoveMonth(-1); break;
                case CalendarZoomLevel.Year: MoveYear(-1); break;
                case CalendarZoomLevel.Decade: decadeStartYear -= 12; break;
            }
            return;
        }
        else if (Raylib.CheckCollisionPointRec(mousePos, nextMonthRect))
        {
            switch (CalendarZoom)
            {
                case CalendarZoomLevel.Month: MoveMonth(1); break;
                case CalendarZoomLevel.Year: MoveYear(1); break;
                case CalendarZoomLevel.Decade: decadeStartYear += 12; break;
            }
            return;
        }

        // Zoom out button in the center of header
        if (Raylib.CheckCollisionPointRec(mousePos, zoomOutRect))
        {
            switch (CalendarZoom)
            {
                case CalendarZoomLevel.Month: CalendarZoom = CalendarZoomLevel.Year; break;
                case CalendarZoomLevel.Year: CalendarZoom = CalendarZoomLevel.Decade; break;
                default: break;
            }
            return;
        }
    }

    private void HandleAmPmClick(Vector2 mousePos)
    {
        if (Use24Hour) return; // nothing to do in 24h mode

        Rectangle amRect = new Rectangle(timePanelRect.X + 10, timePanelRect.Y + 8, 48, 20);
        Rectangle pmRect = new Rectangle(timePanelRect.X + timePanelRect.Width - 58, timePanelRect.Y + 8, 48, 20);

        if (Raylib.CheckCollisionPointRec(mousePos, amRect))
        {
            var dt = selected;
            if (dt.Hour >= 12) dt = dt.AddHours(-12);
            Selected = dt;
        }
        else if (Raylib.CheckCollisionPointRec(mousePos, pmRect))
        {
            var dt = selected;
            if (dt.Hour < 12) dt = dt.AddHours(12);
            Selected = dt;
        }
    }

    private void HandleClockFaceClick(Vector2 mousePos)
    {
        float cx = timePanelRect.X + timePanelRect.Width * 0.5f;
        float cy = timePanelRect.Y + timePanelRect.Height * 0.5f + 8;
        float radius = Math.Min(timePanelRect.Width, timePanelRect.Height) * CLOCK_RADIUS_RATIO;

        float dx = mousePos.X - cx;
        float dy = mousePos.Y - cy;
        float dist = (float)Math.Sqrt(dx * dx + dy * dy);

        if (dist <= radius * 1.05f) // click inside clock
        {
            isDraggingClock = true;
            UpdateClockSelectionFromPoint(mousePos.X, mousePos.Y, true); // pick time immediately
        }
    }



    private void HandleCalendarDayClick(Vector2 mousePos)
    {
        int firstWeek = DayOfWeekIndex(new DateTime(visibleMonth.Year, visibleMonth.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(visibleMonth.Year, visibleMonth.Month);

        for (int i = 0; i < dayCellRects.Count; i++)
        {
            if (Raylib.CheckCollisionPointRec(mousePos, dayCellRects[i]))
            {
                int dayIndex = i - firstWeek + 1;
                if (dayIndex >= 1 && dayIndex <= daysInMonth)
                {
                    // preserve time portion
                    var newDT = new DateTime(visibleMonth.Year, visibleMonth.Month, dayIndex, selected.Hour, selected.Minute, 0);
                    Selected = newDT;
                }
                break;
            }
        }
    }

    private void HandleCalendarCellClick(Vector2 mousePos)
    {
        switch (CalendarZoom)
        {
            case CalendarZoomLevel.Month:
                HandleCalendarDayClick(mousePos); // picks a day
                break;
            case CalendarZoomLevel.Year:
                for (int i = 0; i < 12; i++)
                {
                    Rectangle cell = GetYearCellRect(i); // method returning rectangle for month i
                    if (Raylib.CheckCollisionPointRec(mousePos, cell))
                    {
                        visibleMonth = new DateTime(visibleMonth.Year, i + 1, 1);
                        CalendarZoom = CalendarZoomLevel.Month;
                        break;
                    }
                }
                break;
            case CalendarZoomLevel.Decade:
                for (int i = 0; i < 12; i++)
                {
                    int year = decadeStartYear + i;
                    Rectangle cell = GetDecadeCellRect(i); // rectangle for year i in decade
                    if (Raylib.CheckCollisionPointRec(mousePos, cell))
                    {
                        visibleMonth = new DateTime(year, visibleMonth.Month, 1);
                        CalendarZoom = CalendarZoomLevel.Year;
                        break;
                    }
                }
                break;
        }
    }

    // For the year view: 12 months arranged in 3x4 grid
    private Rectangle GetYearCellRect(int monthIndex)
    {
        int cols = 3;
        int rows = 4;
        float cellWidth = calendarRect.Width / cols;
        float cellHeight = (calendarRect.Height - MONTH_HEADER_HEIGHT) / rows; // subtract header

        int col = monthIndex % cols;
        int row = monthIndex / cols;

        return new Rectangle(
            calendarRect.X + col * cellWidth,
            calendarRect.Y + MONTH_HEADER_HEIGHT + row * cellHeight, // offset by header
            cellWidth,
            cellHeight
        );
    }

    private Rectangle GetDecadeCellRect(int yearIndex)
    {
        int cols = 3;
        int rows = 4;
        float cellWidth = calendarRect.Width / cols;
        float cellHeight = (calendarRect.Height - MONTH_HEADER_HEIGHT) / rows; // subtract header

        int col = yearIndex % cols;
        int row = yearIndex / cols;

        return new Rectangle(
            calendarRect.X + col * cellWidth,
            calendarRect.Y + MONTH_HEADER_HEIGHT + row * cellHeight, // offset by header
            cellWidth,
            cellHeight
        );
    }


    private void UpdateClockSelectionFromPoint(float mouseX, float mouseY, bool commit)
    {
        float cx = timePanelRect.X + timePanelRect.Width * 0.5f;
        float cy = timePanelRect.Y + timePanelRect.Height * 0.5f + 8;
        float dx = mouseX - cx;
        float dy = mouseY - cy;

        double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        double shifted = (angle + 90.0 + 360.0) % 360.0; // 0 = top, 360 wrap

        if (selectingHours)
        {
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            bool useOuter = dist >= (innerHourRadius + outerHourRadius) / 2f; // outer ring if further out

            if (Use24Hour)
            {
                // 24h: outer ring 12–23, inner ring 0–11
                int hourIndex = (int)Math.Round(shifted / 30.0) % 12; // 0..11
                int newHour = useOuter ? hourIndex + 12 : hourIndex;  // outer = 12–23, inner = 0–11

                if (commit)
                {
                    try
                    {
                        Selected = new DateTime(
                            selected.Year, selected.Month,
                            Math.Clamp(selected.Day, 1, DateTime.DaysInMonth(selected.Year, selected.Month)),
                            newHour, selected.Minute, 0);
                    }
                    catch { }
                }
                hoveredHour = newHour;
            }
            else
            {
                // 12h mode as before
                int hourIndex = (int)Math.Round(shifted / 30.0) % 12;
                int chosen = (hourIndex == 0) ? 12 : hourIndex;
                bool isPM = selected.Hour >= 12;
                int newHour = isPM ? (chosen % 12) + 12 : (chosen % 12);

                if (commit)
                {
                    try
                    {
                        Selected = new DateTime(selected.Year, selected.Month,
                            Math.Clamp(selected.Day, 1, DateTime.DaysInMonth(selected.Year, selected.Month)),
                            newHour, selected.Minute, 0);
                    }
                    catch { }
                }
                hoveredHour = chosen;
            }
        }
        else
        {
            // minutes always full ring: 360/60 = 6deg per minute
            int minuteIndex = (int)Math.Round(shifted / 6.0) % 60;
            if (commit)
            {
                try
                {
                    Selected = new DateTime(
                        selected.Year, selected.Month,
                        Math.Clamp(selected.Day, 1, DateTime.DaysInMonth(selected.Year, selected.Month)),
                        selected.Hour, minuteIndex, 0);
                }
                catch { }
            }
            hoveredMinute = minuteIndex;
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // clicking outside should end any in-progress clock drag
        isDraggingClock = false;
    }
}
