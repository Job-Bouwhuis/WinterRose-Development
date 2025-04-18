using System;
using System.Collections.Generic;
using System.Linq;
using flag = ImGuiNET.ImGuiWindowFlags;
using ImGuiNET;
using vec2 = System.Numerics.Vector2;
using System.Reflection;
using System.Runtime.ExceptionServices;
using WinterRose.Monogame.Worlds;
using Microsoft.Xna.Framework;
using WinterRose.Exceptions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace WinterRose.Monogame;

/// <summary>
/// Provides easy methods to write logs to the game screen at runtime, and to view details about any unhandled exception<br></br><br></br>
/// 
/// You can log string messages using <see cref="Log(object)"/>, <see cref="LogWarning(object)"/>, and <see cref="LogError(object)"/>. these objects will have automatic.ToString() be called on them<br></br>
/// You can log an <see cref="Exception"/> using <see cref="LogException(Exception)"/> <b>NOTE:</b> this will stop your entire game and only show windows with details about the exception.
/// </summary>
public static class Debug
{
    /// <summary>
    /// When true, never shows the debug window when in debug mode
    /// </summary>
    public static bool NeverShowInDebugMode { get; set; } = false;
    /// <summary>
    /// When true, always shows the debug window when in debug mode, regardless if there are logs to be shown
    /// </summary>
    public static bool AlwaysShowInDebugMode { get; set; } = false;
    /// <summary>
    /// When true, never shows the debug window when in release mode
    /// </summary>
    public static bool NeverShowInRelease { get; set; } = true;
    /// <summary>
    /// When true, always shows the debug window when in release mode, regardless if there are logs to be shown
    /// </summary>
    public static bool AlwaysShowInRelease { get; set; } = false;

    /// <summary>
    /// Also write the logs to the console instead of just the ingame debug window
    /// </summary>
    public static bool AlsoUseConsole { get; set; } = false;
    /// <summary>
    /// Whether an error was thrown at this time
    /// </summary>
    public static bool Errorred => logs.Any(x => x.LogType == LogMessageType.Error);
    /// <summary>
    /// Whether an exception was thrown. Use the Try-Catch clause to avoid the exception coming here and stopping your entire game
    /// </summary>
    public static bool ExceptionThrown => exceptions.Count > 0;
    /// <summary>
    /// Auto clear logs each frame
    /// </summary>
    public static bool AutoClear { get; set; } = true;
    /// <summary>
    /// True when an exception is thrown, and the "Throw this error" button is pressed in the exception window
    /// </summary>
    public static bool AllowThrow { get; internal set; }

    private static int buttonsOnSameLine = 4;
    private static List<ExceptionLog> exceptions = new();
    private static List<InnerExceptionLog> innerExceptions = new();
    private static List<LogMessage> logs = new();
    private static List<DebugWindowButton> debugButtons = new();
    private static List<Sprite> debugSprites = new();
    private static List<Sprite> ImGuiSprites = new();
    private static List<ExtraWindowSpriteData> ExtraSpriteWindows = new();
    private static List<CachedTexture> DebugDrawRequests = [];
    //private static List<DebugWindowSlider> debugSlideres = new();

    /// <summary>
    /// Clears everything from the debug window, including logs, buttons, and textures
    /// </summary>
    public static void Clear()
    {
        logs.Clear();
        debugButtons.Clear();
        debugSprites.Clear();
        ExtraSpriteWindows.Clear();
        innerExceptions.Clear();
        exceptions.Clear();
    }

    /// <summary>
    /// Logs a message to the debug window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="percistant"></param>
    public static void Log(object message, bool percistant)
    {
        string? s = message?.ToString();
        if (message is null || s is null)
            logs.Add(new("Message was null", LogMessageType.Warning, percistant));
        else
            logs.Add(new(s, LogMessageType.Info, percistant));
    }
    /// <summary>
    /// Logs a message to the debug window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="percistant"></param>
    public static void Log(object message)
    {
        string? s = message?.ToString();
        if (message is null || s is null)
            logs.Add(new("Message was null", LogMessageType.Warning, false));
        else
            logs.Add(new(s, LogMessageType.Info, false));
    }
    /// <summary>
    /// Logs a warning to the debug window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="percistant"></param>
    public static void LogWarning(object message, bool percistant = false)
    {
        string? s = message?.ToString();
        if (message is null || s is null)
            logs.Add(new("Message was null", LogMessageType.Warning, percistant));
        else
            logs.Add(new(s, LogMessageType.Warning, percistant));
    }
    /// <summary>
    /// Logs an error to the debug window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="percistant"></param>
    public static void LogError(object message, bool percistant = false)
    {
        string? s = message?.ToString();
        if (message is null || s is null)
            logs.Add(new("Message was null", LogMessageType.Warning, percistant));
        else
            logs.Add(new(s, LogMessageType.Error, percistant));
    }
    /// <summary>
    /// Logs an exception to the debug window
    /// </summary>
    /// <param name="e"></param>
    public static void LogException(Exception e)
    {
        if (e is TargetInvocationException ex)
            exceptions.Add(new(e.InnerException!));
        else if (e is AggregateException x)
            x.InnerExceptions.Foreach(a => LogException(a));
        else
            exceptions.Add(new(e));
        AutoClear = false;
    }
    /// <summary>
    /// Logs an exception to the debug window
    /// </summary>
    /// <param name="message"></param>
    public static void LogException(string message)
    {
        WinterException winterException = new("a string based exception was thrown.");
        winterException.SetStackTrace(message);
        LogException(winterException);
    }
    /// <summary>
    /// Logs a button to the debug window
    /// </summary>
    /// <param name="label"></param>
    /// <param name="OnClick"></param>
    public static void Button(string label, Action OnClick) => debugButtons.Add(new(label, OnClick));
    /// <summary>
    /// Logs a button to the debug window
    /// </summary>
    /// <param name="button"></param>
    public static void Button(DebugWindowButton button) => debugButtons.Add(button);
    /// <summary>
    /// logs a button with no action to the debug window
    /// </summary>
    /// <param name="label"></param>
    public static void Button(string label) => Button(label, delegate { });
    /// <summary>
    /// logs a texture to the debug window
    /// </summary>
    /// <param name="sprite"></param>
    public static void Sprite(Sprite sprite) => debugSprites.Add(sprite);
    /// <summary>
    /// Draws a rectangle inside the game (can theoretically be used for actual game rendering, but is intended for debugging purposes)
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    public static void DrawRectangle(RectangleF rectangle, Color color, int thickness = 1, RenderSpace space = RenderSpace.World)
    {
        CachedRectangleTexture cachedTexture = GetCachedRectangleTexture(rectangle, color, thickness, space);
        DebugDrawRequests.Add(cachedTexture);
    }

    /// <summary>
    /// Draws a line inside the game (can theoretically be used for actual game rendering, but is intended for debugging purposes)
    /// </summary>
    /// <param name="point1">start point</param>
    /// <param name="point2">end point</param>
    /// <param name="color">Color of the line</param>
    /// <param name="thickness">The thickness of the line in pixels</param>
    public static void DrawLine(Vector2 point1, Vector2 point2, Color color, int thickness = 1, RenderSpace space = RenderSpace.World)
    {
        DebugDrawRequests.Add(new CachedLineTexture(point1, point2, color, thickness, space));
    }

    public static void DrawCircle(Vector2 center, float radius, Color color, int thickness = 1, RenderSpace space = RenderSpace.World)
    {
        DebugDrawRequests.Add(new CachedCircleTexture(center, radius, color, thickness, space));
    }

    //public static void Slider(string label, Action<float> OnValueChange, float value, float min = 0, float max = 1)
    //{
    //    DebugWindowSlider newSlider = new DebugWindowFloatSlider("Slider Label", delegate (float newValue)
    //    {
    //        // Handle value change here
    //        // The original slider record can be accessed via the 'newSlider' variable
    //        Debug.WriteLine($"Slider Value Changed: {newValue}");
    //    }, value, min, max);

    //    debugSlideres.Add(newSlider);

    //    debugSlideres.Add(new DebugWindowFloatSlider(label, OnValueChange, value, min, max));
    //}
    //public static void Slider(string label, Action<int> OnValueChange, int value, int min = 0, int max = 10)
    //{
    //    debugSlideres.Add(new DebugWindowIntSlider(label, OnValueChange, value, min, max));
    //}

    //private static void CreateSliderChildWindow()
    //{
    //    gui.BeginChild("Debug Buttons", new(gui.GetContentRegionAvail().X / 2, 350), true);
    //    gui.Separator();

    //    foreach(var slider in debugSlideres)
    //    {
    //        if(slider is DebugWindowFloatSlider floatSlider)
    //        {
    //            if (gui.SliderFloat(slider.label, ref floatSlider.value, floatSlider.min, floatSlider.max))
    //                slider.OnValueChange.DynamicInvoke(floatSlider.value);
    //        }
    //        if(slider is DebugWindowIntSlider intSlider)
    //        {
    //            if (gui.SliderInt(slider.label, ref intSlider.value, intSlider.min, intSlider.max))
    //                slider.OnValueChange.DynamicInvoke(intSlider.value);
    //        }
    //    }

    //    gui.EndChild();
    //}

    internal static void RenderWorldSpaceDrawRequests(SpriteBatch batch)
    {
        for (int i = 0; i < DebugDrawRequests.Count; i++)
        {
            CachedTexture? request = DebugDrawRequests[i];
            if (request.space == RenderSpace.World)
            {
                request?.Draw(batch);
                DebugDrawRequests.RemoveAt(i);
            }
        }
    }

    internal static void RenderScreenSpaceDrawRequests(SpriteBatch batch)
    {
        for (int i = 0; i < DebugDrawRequests.Count; i++)
        {
            CachedTexture? request = DebugDrawRequests[i];
            if (request.space == RenderSpace.Screen)
            {
                request?.Draw(batch);
                DebugDrawRequests.RemoveAt(i);
            }
        }
    }

    internal static void RenderLayout()
    {
        if (ExceptionThrown)
        {
            MonoUtils.Graphics.Clear(Color.DarkGray);
            DrawExceptionWindows();
            if (Hirarchy.Show)
                Hirarchy.RenderLayout();
            if (WorldEditor.Show)
                WorldEditor.RenderLayout();
            return;
        }

#if DEBUG
        if (NeverShowInDebugMode)
            return;
        if (!AlwaysShowInDebugMode)
            if (!logs.Any() && !debugButtons.Any() && !debugSprites.Any())
                return;
#else
        if (NeverShowInRelease)
            return;
        if(!AlwaysShowInRelease)
            if (!logs.Any() && !debugButtons.Any() && !debugSprites.Any())
                return;
#endif

        for (int i = 0; i < logs.Count; i++)
        {

        }
        int width = MathS.Max(logs.Count > 0 ? logs.Max(x => x.Message.Length) : 400, 400);



        gui.Begin("DEBUG WINDOW");

        gui.SetWindowSize(new(width + 100, 500), ImGuiNET.ImGuiCond.Once);

        if (!Hirarchy.Show)
        {
            if (gui.Button("Display World Hirarchy"))
                Hirarchy.Show = true;
        }
        gui.SameLine();
        if (gui.Button("Clear All Logs"))
        {
            Clear();
        }

        var percistentLogs = logs.Where(x => x.percistant).ToArray();
        CreateLogChildWindow();
        gui.Separator();

        if (debugButtons.Any()) // Check if there are buttons to display
        {
            CreateDebugButtonsChildWindow();
            //gui.SameLine();
        }

        //if (debugButtons.Any() && debugSlideres.Any()) // Show separator if both sections exist
        //{
        //    gui.Separator();
        //}

        //if (debugSlideres.Any()) // Check if there are sliders to display
        //{
        //    CreateSliderChildWindow();
        //}

        gui.Separator();
        gui.Spacing();
        CreateTextureChildWindow();
        gui.Text("other info");
        gui.End();

        if (AutoClear)
        {


            debugButtons.Clear();
            logs.Clear();
            debugSprites.Clear();

            percistentLogs.Foreach(logs.Add);
            //debugSlideres.Clear();
        }
    }

    private static void CreateLogChildWindow()
    {
        if (logs.Count() <= 0)
            return;

        gui.BeginChild("Logs", new(gui.GetContentRegionAvail().X, 200), true, flag.AlwaysVerticalScrollbar);
        for (int i = logs.Count - 1; i >= 0; i--)
        {
            LogMessage? item = logs[i];
            if (item.LogType == LogMessageType.Error)
            {
                gui.TextColored(new(1, 0, 0, 1), item.Message);
                if (AlsoUseConsole)
                    ConsoleExtentions.ConsoleS.WriteErrorLine(item.Message);
            }
            else if (item.LogType == LogMessageType.Warning)
            {
                gui.TextColored(new(0.5f, 0.5f, 0, 1), item.Message);
                if (AlsoUseConsole)
                    ConsoleExtentions.ConsoleS.WriteWarningLine(item.Message);
            }
            else
            {
                gui.Text(item.Message);
                if (AlsoUseConsole)
                    Console.WriteLine(item.Message);
            }
        }
        gui.EndChild();
    }
    private static unsafe void CreateTextureChildWindow()
    {
        if (debugSprites.Count <= 0) return;
        gui.BeginChild("Textures", new(gui.GetContentRegionAvail().X, 500));
        foreach (Sprite texture in debugSprites)
        {
            if (texture is null)
            {
                gui.TextColored(new(1, 0, 0, 1), "Null sprite");
                continue;
            }
            vec2 imageSize = new(texture.Width, texture.Height);
            if (imageSize.X > 500 || imageSize.Y > 500)
                imageSize /= 5;

            var a = Universe.imGuiRenderer.BindTexture(texture);

            if (a is nint ptr)
            {
                gui.Image(ptr, imageSize);

                if (gui.IsItemClicked(ImGuiMouseButton.Middle))
                {
                    ExtraSpriteWindows.Add(new(texture));
                }
            }
            else
                gui.TextColored(new(1, 0, 0, 1), "Unable to load Null pointer");
        }

        gui.EndChild();

        CreateExtraSpriteWindows();
    }

    private static void CreateExtraSpriteWindows()
    {
        for (int i = 0; i < ExtraSpriteWindows.Count; i++)
        {
            ExtraWindowSpriteData? data = ExtraSpriteWindows[i];
            var ptr = Universe.imGuiRenderer.BindTexture(data.sprite);
            string name = data.sprite.TexturePath ?? ptr.ToString();
            bool open = true;
            gui.Begin(name, ref open, flag.AlwaysHorizontalScrollbar | flag.AlwaysVerticalScrollbar | flag.NoCollapse);

            gui.SliderFloat("Scale", ref data.scale, 0.01f, 10f);
            gui.SameLine();
            if (gui.Button("Reset Scale"))
                data.scale = 1f;
            vec2 imageSize = new(data.sprite.Width, data.sprite.Height);
            imageSize *= data.scale;


            gui.Image(ptr, imageSize);
            gui.End();

            if (!open)
                ExtraSpriteWindows.Remove(data);
        }
    }

    //Exceptions
    private static void CreateDebugButtonsChildWindow()
    {
        if (debugButtons.Count <= 0)
            return;
        gui.SliderInt("buttons on one line", ref buttonsOnSameLine, 1, 10);
        //Slider("buttons on one line", x => buttonsOnSameLine = x, 1, 10);

        float availableWidth = gui.GetContentRegionAvail().X / 2;
        float buttonWidth = (gui.GetContentRegionAvail().X - gui.GetStyle().ItemSpacing.X * (buttonsOnSameLine - 1)) / buttonsOnSameLine;

        gui.BeginChild("Debug Buttons", new(gui.GetContentRegionAvail().X, 350), true);
        for (int i = 0; i < debugButtons.Count; i++)
        {
            DebugWindowButton button = debugButtons[i];

            if (i % buttonsOnSameLine == 0)
                gui.Separator();

            if (gui.Button(button.label, new(buttonWidth, 0)))
                button.OnClick();

            if ((i + 1) % buttonsOnSameLine != 0 && i != debugButtons.Count - 1)
                gui.SameLine();
        }
        gui.EndChild();
    }
    private static void DrawExceptionWindows()
    {
        foreach (var log in exceptions.Select((e, i) => (e, i)))
            CreateExceptionWindow(log.e, log.i);
        for (int i = 0; i < innerExceptions.Count; i++)
            CreateExceptionWindow(innerExceptions[i], exceptions.Count + i);

        gui.BeginMainMenuBar();

        if (gui.BeginMenu("App"))
        {
            if (gui.MenuItem("Restart"))
            {
                MonoUtils.RestartApp();
            }
            if (gui.MenuItem("Close"))
            {
                ExitHelper.ExitGame();
            }
            gui.EndMenu();
        }
        if (gui.BeginMenu("Editor"))
        {
            if (gui.MenuItem("Open Editor", !WorldEditor.Show))
            {
                WorldEditor.Show = true;
            }
            if (gui.MenuItem("Close Editor", WorldEditor.Show))
            {
                WorldEditor.Show = false;
            }
            gui.EndMenu();
        }
        if (gui.BeginMenu("Hirachy"))
        {
            if (gui.MenuItem("Open Hirachy", !Hirarchy.Show))
            {
                Hirarchy.Show = true;
            }
            if (gui.MenuItem("Close Hirachy", Hirarchy.Show))
            {
                Hirarchy.Show = false;
            }
            gui.EndMenu();
        }

        gui.EndMainMenuBar();
    }
    private static void CreateExceptionWindow(ExceptionLog log, int num)
    {
        gui.Begin($"Exception: {log.e.GetType().Name} {num}", flag.HorizontalScrollbar);
        gui.SetWindowSize(new(500, 600), ImGuiCond.Once);
        gui.Text($"Source: {log.e.Source}");
        gui.TextWrapped($"Message: {log.e.Message}");

        vec2 stackTraceSize = new(gui.GetContentRegionAvail().X, gui.GetContentRegionAvail().Y - 100);
        if (log.e.StackTrace is null)
            stackTraceSize = stackTraceSize with { X = 150, Y = 50 };
        gui.BeginChild("Stack Trace", new(gui.GetContentRegionAvail().X, 200), true);
        gui.TextWrapped(log.e.StackTrace ?? "No stack trace");
        gui.EndChild();

        if (log.e.InnerException is not null)
            gui.Checkbox("Show inner exception in new window", ref log.DrawInnerException);

        bool debuggerAttached = System.Diagnostics.Debugger.IsAttached;
        if (!debuggerAttached)
        {
            gui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            gui.Button("Throw this error");
            gui.PopStyleVar();
            if (gui.BeginItemTooltip())
            {
                gui.Text("No debugger attached, cant throw the error");
                gui.EndTooltip();
            }

        }
        else if (gui.Button("Throw this error"))
            log.Throw();

        gui.SameLine();
        bool onWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        if (onWindows)
        {
            if (gui.Button("Copy Message"))
            {
                Windows.Clipboard.Clear();
                Windows.Clipboard.WriteString(log.e.Message);
            }
            if (gui.BeginItemTooltip())
            {
                gui.Text("Overrides whatever is in your clipboard right now.");
                gui.EndTooltip();
            }
            gui.SameLine();
            if (log.e.StackTrace is not null)
                if (gui.Button("Copy Stack Trace"))
                {
                    Windows.Clipboard.Clear();
                    Windows.Clipboard.WriteString(log.e.StackTrace);
                }
            if (gui.BeginItemTooltip())
            {
                gui.Text("Overrides whatever is in your clipboard right now.");
                gui.EndTooltip();
            }
        }
        else
        {
            gui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            gui.Button("Copy Message");
            if (gui.BeginItemTooltip())
            {
                gui.Text("Copying is only available on windows.");
                gui.EndTooltip();
            }
            gui.SameLine();
            gui.Button("Copy Stack Trace");
            if (gui.BeginItemTooltip())
            {
                gui.Text("Copying is only available on windows.");
                gui.EndTooltip();
            }
            gui.PopStyleVar();
        }
        gui.Text("End of exception info");
        gui.End();

        if (log.DrawInnerException)
        {
            if (innerExceptions.Any(x => x.parent == log))
                return;

            innerExceptions.Add(new(log, log.e.InnerException!));
        }
        else
        {
            if (log.e.InnerException is null)
                return;
            RemoveInnerExceptions(log);
        }
    }
    private static void RemoveInnerExceptions(ExceptionLog log)
    {
        var exceptions = innerExceptions.Where(x => x.parent == log).ToArray();
        for (int i = 0; i < exceptions.Length; i++)
        {
            var list = innerExceptions.Where(x => x.parent == exceptions[i]).ToArray();
            for (int j = 0; j < list.Length; j++)
            {
                InnerExceptionLog? exception = list[j];
                RemoveInnerExceptions(exception);
                innerExceptions.Remove(exception);
            }
            innerExceptions.Remove(exceptions[i]);
        }
    }

    internal static void ClearExceptions()
    {
        exceptions.Clear();
    }


    private record CachedLineTexture(Vector2 point1, Vector2 point2, Color color, int thickness, RenderSpace space) : CachedTexture(space)
    {
        public override void Draw(SpriteBatch batch)
        {
            batch.DrawLine(point1, point2, color, thickness);
        }
    }
    private record CachedCircleTexture(Vector2 center, float radius, Color color, int thickness, RenderSpace space) : CachedTexture(space)
    {
        public override void Draw(SpriteBatch batch)
        {
            batch.DrawCircle(center, radius, color, thickness);
        }
    }
    private abstract record CachedTexture(RenderSpace space)
    {
        public abstract void Draw(SpriteBatch batch);
    }

    static List<CachedTexture> textureCache = new();

    private record CachedRectangleTexture(RectangleF rectangle, int thickness, Color color, RenderSpace space) : CachedTexture(space)
    {
        public override void Draw(SpriteBatch batch)
        {
            // Top line (left to right)
            batch.DrawLine(
                 new Vector2(rectangle.X, rectangle.Y),
                 new Vector2(rectangle.Right, rectangle.Y),
                color,
                thickness,
                1f
            );

            // Bottom line (left to right)
            batch.DrawLine(
                new Vector2(rectangle.X, rectangle.Bottom),
                new Vector2(rectangle.Right, rectangle.Bottom),
                color: color,
                thickness,
                1f
            );

            // Left line (top to bottom)
            batch.DrawLine(
                new Vector2(rectangle.X, rectangle.Y),
                new Vector2(rectangle.X, rectangle.Bottom),
                color: color,
                thickness,
                1f
            );

            // Right line (top to bottom)
            batch.DrawLine(
                new Vector2(rectangle.Right, rectangle.Y),
                new Vector2(rectangle.Right, rectangle.Bottom),
                color: color,
                thickness,
                1f
            );
        }
    }

    private static CachedRectangleTexture GetCachedRectangleTexture(RectangleF rect, Color color, int thickness, RenderSpace space)
    {
        // Check if the texture is already in the cache based on size, position, thickness, and color
        CachedRectangleTexture? cachedTexture = textureCache.Find(tex =>
        {
            if (tex is not CachedRectangleTexture rec)
                return false;

            bool isThicknessCorrect = rec.thickness == thickness;
            bool isColorCorrect = rec.color == color;

            // Add position check
            bool isPositionCorrect = rec.rectangle.X == rect.X && rec.rectangle.Y == rect.Y &&
                                     rec.rectangle.Width == rect.Width && rec.rectangle.Height == rect.Height;

            return isThicknessCorrect && isColorCorrect && isPositionCorrect && tex.space == space; // Compare positions and attributes
        }) as CachedRectangleTexture;

        // If not found, create and add to the cache
        if (cachedTexture is null)
        {
            cachedTexture = new CachedRectangleTexture(rect, thickness, color, space);
            textureCache.Add(cachedTexture);
        }

        return cachedTexture;
    }

    //public abstract record DebugWindowSlider(string label, Delegate OnValueChange);
    //public record DebugWindowIntSlider(string label, Delegate OnValueChange, int value, int min = 0, int max = 10) : DebugWindowSlider(label, OnValueChange)
    //{
    //    public int value;
    //}
    //public record DebugWindowFloatSlider(string label, Delegate OnValueChange, float value, float min = 0, float max = 1) : DebugWindowSlider(label, OnValueChange)
    //{
    //    public float value;
    //}
    /// <summary>
    /// A debug window button
    /// </summary>
    /// <param name="label">The label of the button</param>
    /// <param name="OnClick">The action to invoke when the button is clicked</param>
    public record DebugWindowButton(string label, Action OnClick);
        /// <summary>
        /// A message to show in the debug window
        /// </summary>
        /// <param name="Message">The message</param>
        /// <param name="LogType">Info, Warning, or Error</param>
        /// <param name="percistant">Wether or not the message should be cleared after the current frame or not</param>
        public record LogMessage(string Message, LogMessageType LogType, bool percistant);
        /// <summary>
        /// Data for a extra window to show a sprite
        /// </summary>
        /// <param name="sprite"></param>
        public record ExtraWindowSpriteData(Sprite sprite)
        {
            /// <summary>
            /// scale of the sprite
            /// </summary>
            public float scale = 1;
        }
        /// <summary>
        /// Info about a logged exception
        /// </summary>
        public class ExceptionLog
        {
            /// <summary>
            /// The captured exception
            /// </summary>
            public readonly ExceptionDispatchInfo info;
            /// <summary>
            /// The exception
            /// </summary>
            public readonly Exception e;
            /// <summary>
            /// Wehter the checkbox "Show inner exception" is checked
            /// </summary>
            public bool DrawInnerException = false;

            public ExceptionLog(Exception e)
            {
                this.e = e;
                info = ExceptionDispatchInfo.Capture(e);
            }

            /// <summary>
            /// Throws the exception using the captured stack trace. only works in debug build type
            /// </summary>
            public void Throw()
            {
                Debug.AllowThrow = true;
                info.Throw();
            }
        }
        /// <summary>
        /// A log of an inner exception
        /// </summary>
        public class InnerExceptionLog : ExceptionLog
        {
            public ExceptionLog parent;
            public InnerExceptionLog(ExceptionLog parent, Exception e) : base(e) => this.parent = parent;
        }
        /// <summary>
        /// type of a log message
        /// </summary>
        public enum LogMessageType
        {
            /// <summary>
            /// Its just (debug) information
            /// </summary>
            Info,
            /// <summary>
            /// Its a warning about something
            /// </summary>
            Warning,
            /// <summary>
            /// Its an error that happened
            /// </summary>
            Error
        }
        /// <summary>
        /// Not in use
        /// </summary>
        [Experimental("WR_0002")]
        public enum DebugSliderType
        {
            /// <summary>
            /// The slider should be using a float value
            /// </summary>
            Float,
            /// <summary>
            /// The slider should be using an integer value
            /// </summary>
            Integer
        }
    }
