using PuppeteerSharp;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes.Boxes;
public class BrowserDialog : Dialog
{
    private IBrowser browser => Application.Current.browser;
    private Texture2D? webTexture;
    private DateTime lastUpdateTime = DateTime.MinValue;
    private const float updateInterval = .1f; // seconds between updates
    private IPage? page;

    private Texture2D? faviconTexture;
    private string? currentFaviconPath;

    // Thread-safe buffer for pending screenshot
    private byte[]? pendingScreenshot;
    private readonly object screenshotLock = new();

    public BrowserDialog(string link,
                         DialogPlacement placement = DialogPlacement.CenterSmall,
                         DialogPriority priority = DialogPriority.Normal)
        : base("Unknown", "", placement, priority, null, null, null)
    {
        InitializeBrowserPage(link);

        Buttons.Add(new DialogButton("Close", () =>
        {
            if (page != null)
            {
                _ = page.CloseAsync();
                page = null;
            }
            Close();
        }));
    }

    private async void InitializeBrowserPage(string link)
    {
        if (browser == null) return;

        page = await browser.NewPageAsync();

        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = (int)Bounds.Width - 100,
            Height = (int)Bounds.Height - 200
        });

        await page.GoToAsync(link);

        // Initial title update
        UpdateTitle(await page.GetTitleAsync());

        // Subscribe to title changes
        page.Console += (sender, e) => 
        {
            Console.WriteLine("BROWSER: " + e.Message.Text);
        }; 
        page.DOMContentLoaded += async (sender, e) =>
        {
            string title = await page.GetTitleAsync();
            UpdateTitle(title);
            await UpdateFaviconAsync();
        };

        // Capture initial screenshot
        _ = CaptureScreenshotAsync();
    }

    private void UpdateTitle(string title)
    {
        int fontSize = Title.FontSize;
        string t = string.IsNullOrWhiteSpace(title) ? "Unknown" : title;
        if (!string.IsNullOrWhiteSpace(currentFaviconPath))
            title += $"\\s[{currentFaviconPath}]\\!";
        Title = title;
        Title.FontSize = fontSize;
    }

    private async Task UpdateFaviconAsync()
    {
        if (page == null) return;

        string? faviconUrl = await page.EvaluateExpressionAsync<string>(
            @"(() => {
            const icon = document.querySelector('link[rel~=""icon""]');
            return icon ? icon.href : null;
        })()"
        );

        if (string.IsNullOrWhiteSpace(faviconUrl) || faviconUrl == currentFaviconPath) return;

        try
        {
            // Dispose previous texture
            if (faviconTexture.HasValue)
            {
                Raylib_cs.Raylib.UnloadTexture(faviconTexture.Value);
                faviconTexture = null;
            }

            // Download favicon
            using var client = new System.Net.Http.HttpClient();
            byte[] data = await client.GetByteArrayAsync(faviconUrl);

            // Save temporary file
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ico");
            await File.WriteAllBytesAsync(tempPath, data);
            currentFaviconPath = tempPath;

            // Load into Raylib
            Raylib_cs.Image img = Raylib_cs.Raylib.LoadImage(tempPath);
            faviconTexture = Raylib_cs.Raylib.LoadTextureFromImage(img);
            RichSpriteRegistry.RegisterSprite(
                "favicon",
                new(faviconTexture.Value, false)
            );
            Raylib_cs.Raylib.UnloadImage(img);
        }
        catch
        {
            // ignore favicon loading errors
        }
    }

    private void DisposeFavicon()
    {
        if (faviconTexture.HasValue)
        {
            Raylib_cs.Raylib.UnloadTexture(faviconTexture.Value);
            faviconTexture = null;
        }

        if (!string.IsNullOrWhiteSpace(currentFaviconPath) && File.Exists(currentFaviconPath))
        {
            File.Delete(currentFaviconPath);
            currentFaviconPath = null;
        }
    }

    private static readonly Dictionary<KeyboardKey, string> RaylibToPuppeteerKey = new()
{
    // Letters A-Z
    { Raylib_cs.KeyboardKey.A, "a" },
    { Raylib_cs.KeyboardKey.B, "b" },
    { Raylib_cs.KeyboardKey.C, "c" },
    { Raylib_cs.KeyboardKey.D, "d" },
    { Raylib_cs.KeyboardKey.E, "e" },
    { Raylib_cs.KeyboardKey.F, "f" },
    { Raylib_cs.KeyboardKey.G, "g" },
    { Raylib_cs.KeyboardKey.H, "h" },
    { Raylib_cs.KeyboardKey.I, "i" },
    { Raylib_cs.KeyboardKey.J, "j" },
    { Raylib_cs.KeyboardKey.K, "k" },
    { Raylib_cs.KeyboardKey.L, "l" },
    { Raylib_cs.KeyboardKey.M, "m" },
    { Raylib_cs.KeyboardKey.N, "n" },
    { Raylib_cs.KeyboardKey.O, "o" },
    { Raylib_cs.KeyboardKey.P, "p" },
    { Raylib_cs.KeyboardKey.Q, "q" },
    { Raylib_cs.KeyboardKey.R, "r" },
    { Raylib_cs.KeyboardKey.S, "s" },
    { Raylib_cs.KeyboardKey.T, "t" },
    { Raylib_cs.KeyboardKey.U, "u" },
    { Raylib_cs.KeyboardKey.V, "v" },
    { Raylib_cs.KeyboardKey.W, "w" },
    { Raylib_cs.KeyboardKey.X, "x" },
    { Raylib_cs.KeyboardKey.Y, "y" },
    { Raylib_cs.KeyboardKey.Z, "z" },

    // Control keys
    { Raylib_cs.KeyboardKey.Enter, "Enter" },
    { Raylib_cs.KeyboardKey.Backspace, "Backspace" },
    { Raylib_cs.KeyboardKey.Tab, "Tab" },
    { Raylib_cs.KeyboardKey.Delete, "Delete" },
    { Raylib_cs.KeyboardKey.Escape, "Escape" },
    { Raylib_cs.KeyboardKey.CapsLock, "CapsLock" },
    { Raylib_cs.KeyboardKey.LeftShift, "Shift" },
    { Raylib_cs.KeyboardKey.RightShift, "Shift" },
    { Raylib_cs.KeyboardKey.LeftControl, "Control" },
    { Raylib_cs.KeyboardKey.RightControl, "Control" },
    { Raylib_cs.KeyboardKey.LeftAlt, "Alt" },
    { Raylib_cs.KeyboardKey.RightAlt, "Alt" },

    // Arrow keys
    { Raylib_cs.KeyboardKey.Up, "ArrowUp" },
    { Raylib_cs.KeyboardKey.Down, "ArrowDown" },
    { Raylib_cs.KeyboardKey.Left, "ArrowLeft" },
    { Raylib_cs.KeyboardKey.Right, "ArrowRight" },

    // Function keys
    { Raylib_cs.KeyboardKey.F1, "F1" },
    { Raylib_cs.KeyboardKey.F2, "F2" },
    { Raylib_cs.KeyboardKey.F3, "F3" },
    { Raylib_cs.KeyboardKey.F4, "F4" },
    { Raylib_cs.KeyboardKey.F5, "F5" },
    { Raylib_cs.KeyboardKey.F6, "F6" },
    { Raylib_cs.KeyboardKey.F7, "F7" },
    { Raylib_cs.KeyboardKey.F8, "F8" },
    { Raylib_cs.KeyboardKey.F9, "F9" },
    { Raylib_cs.KeyboardKey.F10, "F10" },
    { Raylib_cs.KeyboardKey.F11, "F11" },
    { Raylib_cs.KeyboardKey.F12, "F12" },

    // Symbols / punctuation
    { Raylib_cs.KeyboardKey.Semicolon, ";" },
    { Raylib_cs.KeyboardKey.Comma, "," },
    { Raylib_cs.KeyboardKey.Period, "." },
    { Raylib_cs.KeyboardKey.Slash, "/" },
    { Raylib_cs.KeyboardKey.Backslash, "\\" },
    { Raylib_cs.KeyboardKey.Apostrophe, "'" },
    { Raylib_cs.KeyboardKey.Minus, "-" },
    { Raylib_cs.KeyboardKey.Equal, "=" },
    { Raylib_cs.KeyboardKey.LeftBracket, "[" },
    { Raylib_cs.KeyboardKey.RightBracket, "]" },
    { Raylib_cs.KeyboardKey.Grave, "`" },

    // Optional: more symbols
    { Raylib_cs.KeyboardKey.Space, " " },
};


    public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
    {
        if (!webTexture.HasValue) return;

        HandleInput(bounds, y, padding);

        float maxWidth = bounds.Width - padding * 2;

        // Calculate how much vertical space is left below y, minus padding and button area
        float reservedForButtons = 30 + 25 + padding; // same as your sprite example
        float maxHeight = bounds.Height - (y - bounds.Y) - reservedForButtons;

        float aspect = (float)webTexture.Value.Width / webTexture.Value.Height;

        float targetWidth = maxWidth;
        float targetHeight = targetWidth / aspect;

        if (targetHeight > maxHeight)
        {
            targetHeight = maxHeight;
            targetWidth = targetHeight * aspect;
        }

        float drawX = bounds.X + (bounds.Width - targetWidth) / 2;
        float drawY = y;

        Raylib_cs.Raylib.DrawTexturePro(
            webTexture.Value,
            new Rectangle(0, 0, webTexture.Value.Width, webTexture.Value.Height),
            new Rectangle(drawX, drawY, targetWidth, targetHeight),
            new Vector2(0, 0),
            0f,
            Raylib_cs.Color.White
        );

        y += (targetHeight + padding).CeilingToInt();


    }

    private Rectangle GetWebTextureDrawRect(Rectangle bounds, int y, int padding)
    {
        if (!webTexture.HasValue) return new Rectangle();

        float maxWidth = bounds.Width - padding * 2;
        float reservedForButtons = 30 + 25 + padding;
        float maxHeight = bounds.Height - (y - bounds.Y) - reservedForButtons;

        float aspect = (float)webTexture.Value.Width / webTexture.Value.Height;

        float targetWidth = maxWidth;
        float targetHeight = targetWidth / aspect;

        if (targetHeight > maxHeight)
        {
            targetHeight = maxHeight;
            targetWidth = targetHeight * aspect;
        }

        float drawX = bounds.X + (bounds.Width - targetWidth) / 2;
        float drawY = y;

        return new Rectangle(drawX, drawY, targetWidth, targetHeight);
    }

    public void HandleInput(Rectangle bounds, int y, int padding)
    {
        if (page == null || !webTexture.HasValue) return;

        Vector2 mousePos = Raylib_cs.Raylib.GetMousePosition();
        Rectangle texRect = GetWebTextureDrawRect(bounds, y, padding);

        if (mousePos.X >= texRect.X && mousePos.X <= texRect.X + texRect.Width &&
            mousePos.Y >= texRect.Y && mousePos.Y <= texRect.Y + texRect.Height)
        {
            float scaleX = (float)page.Viewport.Width / texRect.Width;
            float scaleY = (float)page.Viewport.Height / texRect.Height;

            float pageX = (mousePos.X - texRect.X) * scaleX;
            float pageY = (mousePos.Y - texRect.Y) * scaleY;

            if (Raylib_cs.Raylib.IsMouseButtonPressed(Raylib_cs.MouseButton.Left))
            {
                _ = page.Mouse.ClickAsync((decimal)pageX, (decimal)pageY);
            }

            float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                _ = page.Mouse.WheelAsync(0, (decimal)(-wheel * 50));
            }
        }

        // --- KEYBOARD INPUT ---
        while (Raylib_cs.Raylib.GetKeyPressed() is int keyCode && keyCode != 0)
        {
            if (RaylibToPuppeteerKey.TryGetValue((KeyboardKey)keyCode, out string puppeteerKey))
            {
                if (puppeteerKey.Length == 1 &&
                    char.IsLetter(puppeteerKey[0]) &&
                    (Raylib_cs.Raylib.IsKeyDown(KeyboardKey.LeftShift) ||
                     Raylib_cs.Raylib.IsKeyDown(KeyboardKey.RightShift)))
                {
                    puppeteerKey = puppeteerKey.ToUpper();
                }

                _ = page.Keyboard.PressAsync(puppeteerKey);
            }
        }
    }



    public override void Update()
    {
        if (page == null) return;

        // Trigger screenshot capture every update interval
        if ((DateTime.Now - lastUpdateTime).TotalSeconds >= updateInterval)
        {
            _ = CaptureScreenshotAsync(); // async capture
            lastUpdateTime = DateTime.Now;
        }

        // Apply screenshot to texture on main thread
        byte[]? bytesToLoad = null;
        lock (screenshotLock)
        {
            if (pendingScreenshot != null)
            {
                bytesToLoad = pendingScreenshot;
                pendingScreenshot = null;
            }
        }

        if (bytesToLoad != null)
        {
            using var ms = new MemoryStream(bytesToLoad);
            Raylib_cs.Image img = Raylib_cs.Raylib.LoadImageFromMemory(".png", ms.ToArray());

            // Resize to fit dialog bounds if needed
            // Raylib_cs.Raylib.ImageResize(ref img, (int)bounds.width, (int)bounds.height);
            unsafe
            {
                if (webTexture.HasValue)
                {
                    Raylib_cs.Raylib.UpdateTexture(webTexture.Value, img.Data);
                    Raylib_cs.Raylib.UnloadImage(img);
                }
                else
                {
                    webTexture = Raylib_cs.Raylib.LoadTextureFromImage(img);
                    Raylib_cs.Raylib.UnloadImage(img);
                }
            }
        }
    }

    private async Task CaptureScreenshotAsync()
    {
        if (page == null) return;

        byte[] imageBytes = await page.ScreenshotDataAsync();

        // Store safely for main thread application
        lock (screenshotLock)
        {
            pendingScreenshot = imageBytes;
        }
    }
}


