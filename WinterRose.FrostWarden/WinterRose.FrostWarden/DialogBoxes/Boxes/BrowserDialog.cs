using PuppeteerSharp;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using WinterRose.ForgeWarden.TextRendering;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.DialogBoxes.Boxes;

public class BrowserDialog : Dialog
{
    private IBrowser browser => Application.Current.browser;
    private Texture2D? webTexture;
    private DateTime lastUpdateTime = DateTime.MinValue;
    private const float updateInterval = .1f; // seconds between updates
    private IPage? page;

    private bool navigationFailed;
    private string? failedUrl;

    private Texture2D? faviconTexture;
    private string? currentFaviconPath;

    private Task? screenshottingTask;

    // Thread-safe buffer for pending screenshot
    private byte[]? pendingScreenshot;
    private readonly object screenshotLock = new();

    public BrowserDialog(string link,
        DialogPlacement placement = DialogPlacement.CenterSmall,
        DialogPriority priority = DialogPriority.Normal)
        : base("Unknown", "", placement, priority, null, null, null)
    {
        InitializeBrowserPage(link);

        Buttons.Add(new DialogButton("Reload Page", () =>
        {
            _ = page?.ReloadAsync();
            Console.WriteLine("Reload Button");
            return false;
        }));

        Buttons.Add(new DialogButton("Close", () =>
        {
            Console.WriteLine("Close Button");
            if (page != null)
            {
                _ = page.CloseAsync();
                page = null;
            }

            DisposeFavicon();
            return true;
        }));
    }

    private async void InitializeBrowserPage(string link)
    {
        if (browser == null) return;

        try
        {
            page = await browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = (int)Bounds.Width - 100,
                Height = (int)Bounds.Height - 200
            });

            try
            {
                await page.GoToAsync(link);
            }
            catch (NavigationException nex)
            {
                Console.WriteLine($"Navigation failed: {nex.Message}");
                navigationFailed = true;
                failedUrl = link;
                return; // stop initialization here
            }

            UpdateTitle(await page.GetTitleAsync());
            UpdateFaviconAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine("Exception in UpdateFaviconAsync: " + t.Exception);
                }
            });

            page.Console += (sender, e) => { Console.WriteLine("BROWSER: " + e.Message.Text); };
            page.DOMContentLoaded += async (sender, e) =>
            {
                try
                {
                    string title = await page.GetTitleAsync();
                    UpdateTitle(title);
                    UpdateFaviconAsync().ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Console.WriteLine("Exception in UpdateFaviconAsync: " + t.Exception);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in DOMContentLoaded handler: " + ex);
                }
            };

            screenshottingTask = CaptureScreenshotAsync()
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Console.WriteLine("Screenshot task failed: " + t.Exception);
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in InitializeBrowserPage: " + ex);
        }
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

            var client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync(faviconUrl);
            client.Dispose();
            Image img;
            try
            {
                // Try direct load from memory (works for PNG/JPG/BMP)
                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        string ext = Path.GetExtension(faviconUrl) ?? ".png";
                        // Convert string to sbyte* using stackalloc
                        int len = ext.Length;
                        sbyte* extPtr = stackalloc sbyte[len + 1];
                        for (int i = 0; i < len; i++) extPtr[i] = (sbyte)ext[i];
                        extPtr[len] = 0; // null-terminate

                        img = Raylib_cs.Raylib.LoadImageFromMemory(extPtr, ptr, data.Length);
                    }
                }

                goto madeit;
            }
            catch
            {
            }

            {
                // fallback: convert ICO to PNG via ImageSharp
                using var ms = new MemoryStream(data);
                using var imageSharpImage = await SixLabors.ImageSharp.Image.LoadAsync(ms);

                using var outStream = new MemoryStream();
                await ImageExtensions.SaveAsPngAsync(imageSharpImage, outStream, CancellationToken.None);
                outStream.Position = 0;

                byte[] pngBytes = outStream.ToArray();
                unsafe
                {
                    fixed (byte* ptr = pngBytes)
                    {
                        const string ext = ".png";
                        int len = ext.Length;
                        sbyte* extPtr = stackalloc sbyte[len + 1];
                        for (int i = 0; i < len; i++) extPtr[i] = (sbyte)ext[i];
                        extPtr[len] = 0;

                        img = Raylib_cs.Raylib.LoadImageFromMemory(extPtr, ptr, pngBytes.Length);
                    }
                }
            }


            madeit:

            faviconTexture = Raylib_cs.Raylib.LoadTextureFromImage(img);
            Raylib_cs.Raylib.UnloadImage(img);

            RichSpriteRegistry.RegisterSprite(
                "favicon",
                new(faviconTexture.Value, false)
            );

            currentFaviconPath = faviconUrl;
            UpdateTitle(await page.GetTitleAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in UpdateFaviconAsync: " + ex);
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


    public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth,
        ref int y)
    {
        if (navigationFailed)
        {
            string msg = $"Invalid link: {failedUrl}";
            int fontSize = 20;
            Vector2 textSize = Raylib_cs.Raylib.MeasureTextEx(
                Raylib_cs.Raylib.GetFontDefault(),
                msg,
                fontSize,
                1
            );

            float drawX = bounds.X + (bounds.Width - textSize.X) / 2;
            float drawY = y + (bounds.Height - textSize.Y) / 3;

            Raylib_cs.Raylib.DrawTextEx(
                Raylib_cs.Raylib.GetFontDefault(),
                msg,
                new Vector2(drawX, drawY),
                fontSize,
                1,
                new Color(255, 100, 100, (int)(255 * contentAlpha))
            );

            y += (int)(textSize.Y + padding);
            return;
        }

        // standard drawing of the website page
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
                new Color(255, 255, 255, contentAlpha)
            );

            y += (targetHeight + padding).CeilingToInt();
        }
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
                _ = page.Mouse.ClickAsync((decimal)pageX, (decimal)pageY)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Console.WriteLine("Mouse.ClickAsync failed: " + t.Exception);
                    });
            }

            float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                _ = page.Mouse.WheelAsync(0, (decimal)(-wheel * 50))
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Console.WriteLine("Mouse.WheelAsync failed: " + t.Exception);
                    });
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

                _ = page.Keyboard.PressAsync(puppeteerKey)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Console.WriteLine("Keyboard.PressAsync failed: " + t.Exception);
                    });
            }
        }
    }


    public override void Update()
    {
        if (page == null) return;

        // Trigger screenshot capture every update interval
        if ((DateTime.Now - lastUpdateTime).TotalSeconds >= updateInterval)
        {
            if (screenshottingTask is null)
                screenshottingTask = CaptureScreenshotAsync(); // async capture
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

        try
        {
            byte[] imageBytes = await page.ScreenshotDataAsync();
            lock (screenshotLock)
            {
                pendingScreenshot = imageBytes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in CaptureScreenshotAsync: " + ex);
        }
        finally
        {
            screenshottingTask = null;
        }
    }
}