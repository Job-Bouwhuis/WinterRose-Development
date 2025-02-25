using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using WinterRose.FileManagement;
using WinterRose.Monogame.Worlds;
using System.Diagnostics;
using WinterRose.Monogame.RoslynCompiler;
using WinterRose.ConsoleExtentions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics.CodeAnalysis;
using WinterRose.WinterThornScripting;
using WinterRose.Monogame.WinterThornPort;
using WinterRose.Monogame.TextRendering;
using System.Text;

namespace WinterRose.Monogame;

/// <summary>
/// A convenient class containing global elements that can be used throughout the entire project
/// </summary>
public static class MonoUtils
{
    private static Game mainGame;
    internal static readonly BindingFlags InstanceMemberFindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;

    /// <summary>
    /// True if the game is currently in OS focus
    /// </summary>
    public static bool IsActive
    {
        get => mainGame.IsActive;
    }
    /// <summary>
    /// The arguments passed to the process when it was started
    /// </summary>
    public static string[] ProgramArguments
    {
        get
        {
            return Environment.GetCommandLineArgs()[1..];
        }
    }
    /// <summary>
    /// The main game instance. This is <b>NOT</b> intended to 
    /// </summary>
    public static Game MainGame
    {
        get => mainGame;
    }
    /// <summary>
    /// Invoked when the user types on the keyboard<br></br><br></br>
    /// 
    /// event is cleared when new scene loads
    /// </summary>
    public static event Action<TextInputEventArgs> OnUserTextInput = delegate { };
    /// <summary>
    /// Invoked when the game window loses focus
    /// </summary>
    public static event Action OnApplicationFocusLost = delegate { };
    /// <summary>
    /// Invoked when the game window gains focus
    /// </summary>
    public static event Action OnApplicationFocusGained = delegate { };
    /// <summary>
    /// Represents the graphics device manager of this program
    /// </summary>
    public static GraphicsDeviceManager GraphicsManager { get; private set; }
    /// <summary>
    /// Represents the default graphics device used for the game
    /// </summary>
    public static GraphicsDevice Graphics
    {
        get => GraphicsManager.GraphicsDevice;
    }
    /// <summary>
    /// The main game window
    /// </summary>
    public static GameWindow GameWindow
    {
        get => mainGame.Window;
    }

    /// <summary>
    /// Draws the given <see cref="Text"/> on the screen with the given parameters
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="text">The text to be rendered</param>
    /// <param name="origin">The origin of the position, normalized between -1 and 1 for both X and Y</param>
    /// <param name="bounds"></param>
    /// <param name="alignment"></param>
    public static void DrawText(this SpriteBatch batch, Text text, Vector2 origin, RectangleF bounds, TextAlignment alignment)
    {
        float yOffset = bounds.Top;
        float maxLineWidth = bounds.Width;
        float currentLineWidth = 0f;

        // Calculate the base origin point in bounds based on normalized origin values (-1 to 1)
        Vector2 baseOrigin = new Vector2(
            bounds.Left + bounds.Width * origin.X,
            bounds.Top + bounds.Height * origin.Y
        );

        foreach (Word word in text)
        {
            float spaceCharWidth = word.Font.MeasureString(" ").X;
            float xOffset = 0f;
            string lineText = new StringBuilder(word.Count)
                .AppendJoin(string.Empty, word.Select(l => l.Character))
                .ToString();
            var lineSize = word.Font.MeasureString(lineText);
            lineSize.X += spaceCharWidth;

            if (lineText == "\n")
            {
                yOffset += lineSize.Y / 2;
                currentLineWidth = 0f;
                continue;
            }

            switch (alignment)
            {
                case TextAlignment.Left:
                    xOffset = bounds.Left + currentLineWidth;
                    break;
                case TextAlignment.Center:
                    xOffset = bounds.Left + (maxLineWidth - lineSize.X) / 2f;
                    break;
                case TextAlignment.Right:
                    xOffset = bounds.Right - lineSize.X;
                    break;
            }

            Vector2 drawPosition = new Vector2(xOffset, yOffset) - baseOrigin;

            if (currentLineWidth > 0 && currentLineWidth + lineSize.X > maxLineWidth)
            {
                yOffset += lineSize.Y;
                currentLineWidth = 0f;
                drawPosition = new Vector2(bounds.Left, yOffset) - baseOrigin;
            }

            foreach (Letter letter in word)
            {
                if (letter.Character != '\0')
                {
                    batch.DrawString(
                        word.Font,
                        letter.Character.ToString(),
                        drawPosition,
                        letter.Color
                    );
                    drawPosition.X += word.Font.MeasureString(letter.Character.ToString()).X + 1;
                }
            }

            currentLineWidth += lineSize.X + spaceCharWidth;
        }
    }


    /// <summary>
    /// Determines whether the game is running in fullscreen mode
    /// </summary>
    [Experimental("DONT_USE_VERY_BUGGY")]
    public static bool Fullscreen
    {
        get => GraphicsManager.IsFullScreen;
        set
        {
            GraphicsManager.IsFullScreen = value;
            GraphicsManager.ApplyChanges();
        }
    }

    /// <summary>
    /// The size of the actual monitor the game is running on
    /// </summary>
    public static Vector2I ScreenSize
    {
        get
        {
            // return a new Vector2I with the width and height of monitor
            return new Vector2I(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        }
    }

    /// <summary>
    /// represents the width and height of the screen
    /// Applies changes to the graphics manager when set using <see cref="GraphicsManager"/>
    /// </summary>
    public static Vector2I WindowResolution
    {
        get
        {
            return windowResolution;
        }
        set
        {
            windowResolution = value;

            GraphicsManager.PreferredBackBufferWidth = value.X;
            GraphicsManager.PreferredBackBufferHeight = value.Y;
            ScreenCenter = new Vector2I(WindowResolution.X / 2, WindowResolution.Y / 2);
            GraphicsManager.ApplyChanges();
        }
    }
    private static Vector2I windowResolution = new(1280, 720);
    /// <summary>
    /// Whether or not the user can resize the window
    /// </summary>
    public static bool LetUserResizeWindow
    {
        get
        {
            return  GameWindow.AllowUserResizing;
        }
        set
        {
            GameWindow.AllowUserResizing = value;
        }
    }
    /// <summary>
    /// represents the exact coordinates of the center of the screen
    /// </summary>
    public static Vector2I ScreenCenter { get; private set; }
    /// <summary>
    /// represents the default spritebatch used for the game
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }
    /// <summary>
    /// represents the default font used by the game.
    /// </summary>
    public static SpriteFont? DefaultFont { get; set; }
    /// <summary>
    /// the global contentmanager used within the game
    /// </summary>
    public static ContentManager Content { get; private set; }
    /// <summary>
    /// The domain of dynamically loaded scripts. May be null if there are no scripts added this way
    /// </summary>
    public static Assembly? UserDomain { get; internal set; }

    /// <summary>
    /// The viewport of the game
    /// </summary>
    public static Viewport Viewport
    {
        get => Graphics.Viewport;
    }
    /// <summary>
    /// Sets the framerate the game will attempt to run at. set to -1 to set the framerate to unlimited
    /// </summary>
    public static int TargetFramerate
    {
        get
        {
            return _targetFramerate;
        }
        set
        {
            _targetFramerate = value;
            if (value == -1)
            {
                mainGame.IsFixedTimeStep = false;
                mainGame.TargetElapsedTime = new(0, 0, 0, 0, 1);
                return;
            }
            mainGame.IsFixedTimeStep = true;

            try
            {
                TimeSpan time = new();
                time = TimeSpan.FromSeconds(1d / value);
                mainGame.TargetElapsedTime = time;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
    private static int _targetFramerate = 0;
    private static Texture2D? pixel;
    private static int frameRateTargetBeforePause = 0;
    private static bool LostFocus = false;

    /// <summary>
    /// Gets or sets whether the game should use Vsync when the framerate is unlocked
    /// </summary>
    public static bool Vsync
    {
        get => GraphicsManager.SynchronizeWithVerticalRetrace;
        set => GraphicsManager.SynchronizeWithVerticalRetrace = value;
    }
    /// <summary>
    /// retrieves a default 10x10 white texture, this can be used to project upon a rectangle to have a larger white area
    /// </summary>
    public static Texture2D Pixel
    {
        get
        {
            pixel ??= CreateTexture(1, 1, Color.White);
            return pixel;
        }
    }

    public static bool IsCursorVisible
    {
        get => mainGame.IsMouseVisible;
        set => mainGame.IsMouseVisible = value;
    }
    
    public static bool AutoUserdomainUpdate
    {
        get => _autoUserdomainUpdate;
        set
   {
            if (value && !_autoUserdomainUpdate)
            {
                if(!Directory.Exists(UserDomainCompiler.ScriptSource))
                {
                    Debug.LogWarning("User Domain Source Directory does not exist. Creating...");
                    Directory.CreateDirectory(UserDomainCompiler.ScriptSource);
                }

                watcher = new FileSystemWatcher(UserDomainCompiler.ScriptSource);
                watcher.Changed += (s, e) =>
                {
                    Debug.Log(e.FullPath + " changed. Reloading User Domain...");
                    LoadUserDomainWithDefaultSettings();
                };
                watcher.EnableRaisingEvents = true;
                _autoUserdomainUpdate = true;
            }
            if(!value && _autoUserdomainUpdate)
            {
                watcher!.Dispose();
                watcher = null;
                _autoUserdomainUpdate = false;
            }
        }
    }

    public static bool IsStopping { get; internal set; }
    public static RectangleF WindowBounds => new(WindowResolution.X, WindowResolution.Y, 0, 0);

    private static bool _autoUserdomainUpdate = false;
    private static FileSystemWatcher? watcher;
    /// <summary>
    /// Initialize the static utility class. this method <b>MUST</b> be called in order for the WinterRose.Monogame framework to operate properly.<br></br> 
    /// Any method from <see cref="MonoUtils"/> when it is <b>NOT</b> instantiated will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public static void Initialize(Game game, SpriteBatch _spriteBatch, string? fontName = null)
    {
        Content = game.Content;
        Content.RootDirectory = "Content";
        //DirectoryInfo info = new("Content");
        //info.Attributes |= FileAttributes.Hidden;

        GraphicsManager = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
        WindowResolution = new Vector2I(Graphics.Viewport.Bounds.Width, Graphics.Viewport.Bounds.Height);
        SpriteBatch = _spriteBatch;
        try
        {
            DefaultFont = Content.Load<SpriteFont>(fontName);
        }
        catch 
        {
            Debug.LogWarning($"Could not load font with given name {fontName}", true);
        }

        mainGame = game;
        Universe.OnNewWorldLoaded += () =>
        {
            // still need to figure out how to reset the event from OnTextInput.
            // think im gonna make another event for scripts to attach to from monoutils which will
            // get reset to empty when a new scene loads.
            OnUserTextInput = delegate { };
        };
        Time.Setup();
        GameWindow.TextInput += OnTextInput;


        GameWindow.ClientSizeChanged += (s, arg) =>
        {
            Rectangle rect = (Rectangle)s.GetType().GetProperty("ClientBounds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(s);

            WindowResolution = new(rect.Width, rect.Height);
        };
    }
    /// <summary>
    /// Loads the user domain with the default settings
    /// </summary>
    public static void LoadUserDomainWithDefaultSettings()
    {
        UserDomain = null;
        if (!UserDomainCompiler.HasScriptsInSource)
            return;

        var sw = Stopwatch.StartNew();
        UserDomainCompiler.AddAssembly(typeof(MonoUtils));
        UserDomainCompiler.AddAssembly(typeof(WinterUtils));
        UserDomainCompiler.AddAssembly(typeof(Vector2));
        UserDomainCompiler.AddNetCoreDefaultReferences();

        UserDomainCompiler.LoadUserDomain(x => Debug.Log(x));
        Debug.LogWarning("Compiling User Domain...", true);
        UserDomain = UserDomainCompiler.CompileUserDomain(x => 
        { 
            if(x.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            {
                var e = new UserDomainCompilationErrorException();
                var a = x.GetType();
                var b = a.GetProperty("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var c = b.GetValue(x);
                e.SetStackTrace(x.Location.SourceSpan.ToString());
                e.SetMessage(c.ToString());
                Debug.LogException(e);
            }
        });
        sw.Stop();
        if (UserDomain == null)
        {
            Debug.LogError("Encounted fatal when compiling user domain. Please fix compiler errors in your script", true);
        }
    }
    private static void HideAllFiles()
    {
        var a = Process.GetCurrentProcess();
        DirectoryInfo info = new(FileManager.PathOneUp(a.MainModule.FileName));
        foreach (var file in info.EnumerateFiles())
        {
            if (file.Extension is ".exe")
                continue;
            file.Attributes |= FileAttributes.Hidden;
        }
        foreach (var dir in info.EnumerateDirectories())
            dir.Attributes |= FileAttributes.Hidden;
    }
    /// <summary>
    /// Adds the specified value to the enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static T AddFlag<T>(this T e, T flag) where T : Enum
    {
        return (T)(object)((int)(object)e | (int)(object)flag);
    }

    /// <summary>
    /// Adds the specified value to the enum
    /// </summary>
    /// <param name="e"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static Enum AddFlag(this Enum e, Enum flag)
    {
        if (e.GetType() != flag.GetType())
        {
            throw new ArgumentException("Enums must be of the same type");
        }

        var eValue = Convert.ToInt64(e);
        var flagValue = Convert.ToInt64(flag);
        var result = eValue | flagValue;
        var enumResult = (Enum)Enum.ToObject(e.GetType(), result);
        return enumResult;
    }

    /// <summary>
    /// Removes the given value from the enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static T RemoveFlag<T>(this T e, T flag) where T : Enum
    {
        return (T)(object)((int)(object)e & ~(int)(object)flag);
    }
    private static void OnTextInput(object sender, TextInputEventArgs e) => OnUserTextInput(e);
    internal static bool Any(this List<WorldTemplateTypeSearchOverride> list, WorldTemplateTypeSearchOverride def)
    {
        foreach (WorldTemplateTypeSearchOverride t in list)
            if (t.Identifier == def.Identifier && t.Type == def.Type)
                return true;
        return false;
    }
    internal static bool Any(this List<WorldTemplateTypeSearchOverride> list, Type type, out WorldTemplateTypeSearchOverride? overrideDef)
    {
        foreach (WorldTemplateTypeSearchOverride t in list)
            if (t.Type == type)
            {
                overrideDef = t;
                return true;
            }
        overrideDef = null;
        return false;
    }
    /// <summary>
    /// Creates a new texture of the given <paramref name="width"/> and <paramref name="height"/> with the color of the <paramref name="Hex"/> value. this <paramref name="Hex"/> is ordered re
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="Hex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Texture2D CreateTexture(int width, int height, string Hex)
    {
        if (Hex.Length is 7 or 9)
            Hex = Hex.TrimStart('#');

        if (Hex.Length is not 6 and not 8)
            throw new ArgumentException("Hexadecimal must contain either 6 or 8 0-f characters. ordered red-green-blue-alpha", nameof(Hex));

        var bytes = Convert.FromHexString(Hex);
        if (bytes.Length is 4)
            return CreateTexture(width, height, new Color(bytes[0], bytes[1], bytes[2], bytes[3]));
        return CreateTexture(width, height, new Color(bytes[0], bytes[1], bytes[2], (byte)255));
    }
    /// <summary>
    /// Creates a new texture of the given <paramref name="width"/> and <paramref name="height"/> with the colors found in the given 1d <paramref name="bytes"/> array
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Texture2D CreateTexture(int width, int height, byte[] bytes)
    {
        List<Color> colors = new();
        for (int i = 0; i < bytes.Length; i += 4)
        {
            byte b1 = bytes[i];
            byte b2 = bytes[i + 1];
            byte b3 = bytes[i + 2];
            byte b4 = bytes[i + 3];

            colors.Add(new(b1, b2, b3, b4));
        }
        return CreateTexture(width, height, colors.Select(x => x.PackedValue).ToArray());
    }

    public static WorldObjectPrefab CreatePrefab(this WorldObject obj, string name)
    {
        WorldObjectPrefab prefab = new(name, obj);
        return prefab;
    }

    /// <summary>
    /// Converts the given <see cref="Microsoft.Xna.Framework.Vector2"/> to a <see cref="Microsoft.Xna.Framework.Vector3"/>
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 Vector3(this Vector2 vec) => new(vec.X, vec.Y, 0);
    /// <summary>
    /// Converts the <see cref="Microsoft.Xna.Framework.Vector3"/> to a <see cref="Microsoft.Xna.Framework.Vector2"/>
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector2 Vector2(this Vector3 vec) => new(vec.X, vec.Y);
    /// <summary>
    /// Gets a new vector where its values are the absolute values of the given vector
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector2 Abs(this Vector2 vec) => new(MathF.Abs(vec.X), Math.Abs(vec.Y));
    /// <summary>
    /// Deconstructs the given <see cref="Vector2I"/> into its <see cref="Vector2I.X"/> and <see cref="Vector2I.Y"/> values
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void Deconstruct(this Vector2I vec, out int x, out int y)
    {
        x = vec.X;
        y = vec.Y;
    }
    /// <summary>
    /// Deconstructs the given <see cref="Vector2"/> into its <see cref="Vector2.X"/> and <see cref="Vector2.Y"/> values
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void Deconstruct(this Vector2 vec, out float x, out float y)
    {
        x = vec.X;
        y = vec.Y;
    }
    /// <summary>
    /// Deconstructs the given <see cref="Vector3"/> into its <see cref="Vector3.X"/>, <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> values
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public static void Deconstruct(this Vector3 vec, out float x, out float y, out float z)
    {
        x = vec.X;
        y = vec.Y;
        z = vec.Z;
    }
    /// <summary>
    /// Deconstructs the given <see cref="Rectangle"/> into its <see cref="Rectangle.X"/>, <see cref="Rectangle.Y"/>, <see cref="Rectangle.Width"/>, and <see cref="Rectangle.Height"/> values
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void Deconstruct(this Rectangle rect, out int x, out int y, out int width, out int height)
    {
        x = rect.X;
        y = rect.Y;
        width = rect.Width;
        height = rect.Height;
    }
    /// <summary>
    /// checks if the given object is within the boundaries of the game window
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="tex"></param>
    /// <returns>return true if position is within bounds of screensize, accounts for texture sizes</returns>
    [Experimental("WRM_OutOfDate")]
    public static bool AreYouOnScreen(Vector2 pos, Texture2D tex)
    {
        throw new NotImplementedException();
        return pos.X > 0 - tex.Width * 1.01 &&
               pos.X < WindowResolution.X + tex.Width * 1.01 &&
               pos.Y > 0 - tex.Height * 1.01 &&
               pos.Y < WindowResolution.Y + tex.Height * 1.01;
    }
    /// <summary>
    /// whether the position is within the boundaries of the game window
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>true if the position is out of bounds of the screen</returns>
    [Experimental("WRM_OutOfDate")]
    public static bool IsOutOfBounds(Vector2 pos)
    {
        return !(pos.X > 0 &&
            pos.X < WindowResolution.X &&
            pos.X > 0 &&
            pos.X < WindowResolution.Y);
    }
    /// <summary>
    /// Creates a new <see cref="RenderTarget2D"/> using the default Graphics device and the screen with and height
    /// </summary>
    public static RenderTarget2D CreateNewRenderTarget()
    {
        return new(Graphics, (int)WindowResolution.X, (int)WindowResolution.Y);
    }
    /// <summary>
    /// Gets <paramref name="vec"/> as a normalized vector
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector2 Normalized(this Vector2 vec)
    {
        vec.Normalize();
        return vec;
    }
    /// <summary>
    /// Gets a random vector2 with the given min and max values
    /// </summary>
    /// <param name="random"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector2 NextVector2(this Random random, Vector2 X, Vector2 Y)
    {
        return new(random.NextFloat(X.X, X.Y), random.NextFloat(Y.X, Y.Y));
    }
    /// <summary>
    /// Gets a random vector2 with the given min and max values
    /// </summary>
    /// <param name="random"></param>
    /// <param name="minXY"></param>
    /// <param name="maxXY"></param>
    /// <returns></returns>
    public static Vector2 NextVector2(this Random random, float minXY, float maxXY)
    {
        return new(random.NextFloat(minXY, maxXY), random.NextFloat(minXY, maxXY));
    }
    /// <summary>
    /// Creates a new <see cref="RenderTarget2D"/> with the given width and height
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static RenderTarget2D CreateRenderTarget(int width, int height)
    {
        return new(Graphics, width, height);
    }
    /// <summary>
    /// Rounds both the X and Y values of the given vector to only have the given amount of decimals
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public static Vector2 Round(this Vector2 vec, int decimals = 0)
    {
        return new((float)Math.Round(vec.X, decimals), (float)Math.Round(vec.Y, decimals));
    }
    /// <summary>
    /// Creates a new spritebatch using the default graphics device
    /// </summary>
    public static SpriteBatch CreateNewSpriteBatch()
    {
        return new(Graphics);
    }
    /// <summary>
    /// Writes the data of the texture to a file at the given path. creates a new one if one does not exist. closes the file after writing
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="path"></param>
    public static void SaveAsPng(this Texture2D texture, string path)
    {
        Stream stream = File.Open(path, FileMode.OpenOrCreate);
        texture.SaveAsPng(stream, texture.Width, texture.Height);
        stream.Dispose();
    }
    /// <summary>
    /// Creates a new textures using the given parameters
    /// </summary>
    /// <param name="x">the Width of the texture</param>
    /// <param name="y">the Height of the texture</param>
    /// <param name="color">the Color of the texture</param>
    /// <param name="alpha">the transparency of the texture</param>
    /// <param name="_graphics">the graphics device to use when creating this texture</param>
    /// <returns>the newly created texture</returns>
    public static Texture2D CreateTexture(int x, int y, Color color, GraphicsDevice _graphics, byte alpha = 255)
    {
        Texture2D texture;
        texture = new Texture2D(_graphics, x, y);

        try
        {
            Color[] colorData = new Color[x * y];
            color.A = alpha;
            for (int i = 0; i < x * y; i++)
                colorData[i] = color;
            texture.SetData(colorData);
            return texture;
        }
        catch
        {
            //Debug.Log(ex.Message);
            throw;
        }

    }
    /// <summary>
    /// Creates a new texture with the given set of uint color data
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Texture2D CreateTexture(int x, int y, uint[] data)
    {
        Texture2D texture;
        texture = new Texture2D(Graphics, x, y);

        try
        {
            Color[] colorData = new Color[x * y];
            for (int i = 0; i < x * y; i++)
                colorData[i] = new Color(data[i]);
            texture.SetData(colorData);
            return texture;
        }
        catch (Exception ex)
        {
            //Debug.Log(ex.Message);
            throw;
        }
    }
    /// <summary>
    /// Creates a new textures using the given parameters
    /// </summary>
    /// <param name="x">the Width of the texture</param>
    /// <param name="y">the Height of the texture</param>
    /// <param name="color">the Color of the texture</param>
    /// <param name="alpha">the transparency of the texture</param>
    /// <returns>the newly created texture</returns>
    public static Texture2D CreateTexture(int x, int y, Color color, byte alpha = 255)
    {
        Texture2D texture;
        if(x <= 0)  x = 1;
        if(y <= 0)  y = 1;
        texture = new Texture2D(Graphics, x, y);

        try
        {
            Color[] colorData = new Color[x * y];
            color.A = alpha;
            for (int i = 0; i < x * y; i++)
                colorData[i] = color;
            texture.SetData(colorData);
            return texture;
        }
        catch (Exception ex)
        {
            //Debug.Log(ex.Message);
            throw;
        }

    }
    /// <summary>
    /// alters the given texture to have a different color and or alpha. keep in mind that this does not make a copy of the texture, <br></br>
    /// it alters the given texture. all references to the texture will be altered
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="alpha"></param>
    /// <returns>the altered texture</returns>
    public static Texture2D AlterTexture(Texture2D texture, Color color, byte alpha = 255)
    {
        Texture2D tex = texture;
        var colorData = new Color[tex.Width * tex.Height];
        color.A = alpha;
        WinterUtils.Repeat(x => colorData[x] = color, tex.Width * tex.Height);
        tex.SetData(colorData);
        return tex;
    }
    /// <summary>
    /// Sets the game window to the foreground
    /// </summary>
    public static void ActivateWindow()
    {
        Windows.MyHandle.Show();
        Windows.MyHandle.Focus();
    }
    /// <summary>
    /// Restarts the game
    /// </summary>
    public static void RestartApp(params string[] args)
    {
        try
        {
            Debug.Log("Restarting...");
            var a = Process.GetCurrentProcess();
            Process p = Process.Start(a.MainModule.FileName, args);

            if (p is null)
            {
                Debug.LogException("Restarting not possible, Current process was null");
                return;
            }
            ConsoleS.WriteWarningLine("Restarting...");
            MainGame.Exit();
        }
        catch (Exception ex)
        {
            ConsoleS.WriteErrorLine("Couldnt restart: " + ex.Message);
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Draws a circle with the given center, radius, color, and thickness
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int thickness = 1)
    {
        const int CircleResolution = 30; // Increase for smoother circles, but it'll affect performance

        Vector2[] circlePoints = new Vector2[CircleResolution];

        // Calculate points on the circle
        for (int i = 0; i < CircleResolution; i++)
        {
            float angle = MathHelper.TwoPi * i / CircleResolution;
            circlePoints[i] = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        // Draw lines between adjacent points to form the circle with thickness
        for (int i = 0; i < CircleResolution - 1; i++)
        {
            spriteBatch.DrawLine(circlePoints[i], circlePoints[i + 1], color, thickness);
        }

        // Draw final line to close the circle
        spriteBatch.DrawLine(circlePoints[CircleResolution - 1], circlePoints[0], color, thickness);
    }

    public static void DrawBox(this SpriteBatch batch, Vector2 center, Vector2 size, Color color, int thickness = 1)
    {
        Vector2 halfSize = size / 2;
        Vector2 topLeft = center - halfSize;
        Vector2 topRight = new(center.X + halfSize.X, center.Y - halfSize.Y);
        Vector2 bottomLeft = new(center.X - halfSize.X, center.Y + halfSize.Y);
        Vector2 bottomRight = center + halfSize;

        batch.DrawLine(topLeft, topRight, color, thickness);
        batch.DrawLine(topRight, bottomRight, color, thickness);
        batch.DrawLine(bottomRight, bottomLeft, color, thickness);
        batch.DrawLine(bottomLeft, topLeft, color, thickness);
    }

    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int width = 1, float layerdepth = 0.5f)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(Pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), width), null, color, angle, new(), SpriteEffects.None, layerdepth);
    }

    internal static void Activated() => OnApplicationFocusGained();
    internal static void Deactivated() => OnApplicationFocusLost();

    /// <summary>
    /// Creates a <see cref="Namespace"/> for use in the <see cref="WinterThorn"/> scripting language.
    /// <br></br> it contains access to the <see cref="Monogame"/> namespace in the <see cref="WinterThorn"/> scripting language
    /// </summary>
    /// <returns></returns>
    public static Namespace CreateWinterThornNamespace()
    {
        Class input = new InputPort().GetClass();
        Class vector2 = new ThornVector().GetClass();
        Class debug = new ThornDebug().GetClass();
        Class monoutils = new ThornMonoUtils().GetClass();
        Class color = new ThornColor().GetClass();
        Class sprite = new ThornSprite().GetClass();

        return new(
            "WinterRose-ThornPort --- Provides access to the standard WinterRose.Monogame classes from within WinterThorn scripting",
            [
                input, 
                vector2,
                debug,
                monoutils,
                color,
                sprite
            ]);
    }
}