using PuppeteerSharp;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.Recordium;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes;

// taskkill /F /IM chrome.exe

public class BrowserDialog : Dialog, IDisposable
{
    private static volatile int favicoNum = 0;

    private Log log = new Log("Browser");

    private IBrowser browser;
    private Texture2D? webTexture;
    private DateTime lastUpdateTime = DateTime.MinValue;
    private const float updateInterval = .1f; // seconds between updates
    private const string FavIconsPath = @"Data\Browser\Favicons";
    private IPage page;

    private bool navigationFailed;
    private string? failedUrl;

    private int favicoId = favicoNum++;

    private Texture2D? faviconTexture;
    private string? currentFaviconPath;

    private Task? screenshottingTask;

    // Thread-safe buffer for pending screenshot
    private byte[]? pendingScreenshot;
    private bool favIcoUpdateRequired;
    private Image img;
    private readonly object screenshotLock = new();

    private bool IsDisposed = false;

    public BrowserDialog(string link,
        DialogPlacement placement = DialogPlacement.CenterBig,
        DialogPriority priority = DialogPriority.Normal)
        : base("Unknown", "Cookies and logins are \\c[red]NOT\\c[white] saved.", placement, priority)
    {
        if (!Directory.Exists(FavIconsPath))
            Directory.CreateDirectory(FavIconsPath);

        InitializeBrowserPage(link);
        ButtonCreation();

        var titleContent = Contents[0];
        var messageContent = Contents[1];
        Contents.RemoveRange(0, 2);
        Contents.AddRange(titleContent, new UISpacer()
        {
            owner = this
        }, messageContent, new UISpacer()
        {
            owner = this
        });

        SpriteElement = new UISpriteContent();
        AddContent(SpriteElement);

        //AppDomain.CurrentDomain.ProcessExit += PanickedAppClose;
    }

    private void PanickedAppClose(object? sender, EventArgs e) => Dispose();

    private void ButtonCreation()
    {
        AddButton("Reload Page", (container, button) =>
        {
            _ = page?.ReloadAsync();
        });

        AddButton("Reload Dialog", (container, button) =>
        {
            if (page == null) return;

            string urlToReload = failedUrl ?? page.Url;
            BrowserDialog newDialog = new BrowserDialog(urlToReload, Placement, Priority);
            Dialogs.Show(newDialog);
            Close();
        });

        // --- Back Button ---
        AddButton("Back", (container, button) =>
        {
            if (page != null)
            {
                _ = page.GoBackAsync().ContinueWith(t =>
                {
                    if (t.Exception != null)
                        log.Critical(t.Exception, "GoBackAsync failed");
                });
            }
        });

        // --- Forward Button ---
        AddButton("Forward", (container, button) =>
        {
            if (page != null)
            {
                _ = page.GoForwardAsync().ContinueWith(t =>
                {
                    if (t.Exception != null)
                        log.Critical(t.Exception, "GoForwardAsync failed");
                });
            }
        });

        // --- Open in External Browser ---
        AddButton("Open Externally", (container, button) =>
        {
            string urlToOpen = failedUrl ?? page?.Url ?? "";
            if (!string.IsNullOrWhiteSpace(urlToOpen))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = urlToOpen,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to open external browser");
                }
            }
        });

        // --- Copy URL ---
        AddButton("Copy URL", (container, button) =>
        {
            string urlToCopy = failedUrl ?? page?.Url ?? "";
            if (string.IsNullOrWhiteSpace(urlToCopy)) return;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Windows
                    var psi = new ProcessStartInfo("cmd", $"/c echo {urlToCopy} | clip")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Linux, requires xclip or xsel installed
                    var psi = new ProcessStartInfo("bash", $"-c \"echo '{urlToCopy}' | xclip -selection clipboard\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // macOS
                    var psi = new ProcessStartInfo("bash", $"-c \"echo '{urlToCopy}' | pbcopy\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
                else
                {
                     log.Error("Clipboard copy not supported on this OS.");
                } 
            }
            catch (Exception ex)
            {
                log.Critical(ex, "Failed to copy URL");
            }
        });

        AddButton("Close", (container, button) =>
        {
            container.Close();
        });
    }

    private async void InitializeBrowserPage(string link)
    {
        try
        {
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            page = await browser.NewPageAsync();

            if (page == null)
                return;

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = (int)DialogPlacementBounds.Width - 100,
                Height = (int)DialogPlacementBounds.Height - 200
            });

            if (page == null)
                return;

            try
            {
                await page.GoToAsync(link);
            }
            catch (NavigationException nex)
            {
                log.Error(nex, "Navigation failed");
                navigationFailed = true;
                failedUrl = link;
                return; // stop initialization here
            }
            if (page == null)
                return;
            UpdateTitle(await page.GetTitleAsync());
            UpdateFaviconAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                    log.Error(t.Exception, "Exception in UpdateFaviconAsync");
            });

            if (page == null)
                return;

            page.Console += (sender, e) => { log.Debug(e.Message.Text); };
            page.DOMContentLoaded += async (sender, e) =>
            {
                try
                {
                    string title = await page.GetTitleAsync();
                    UpdateTitle(title);
                    UpdateFaviconAsync().ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            log.Warning(t.Exception, "Exception in UpdateFaviconAsync");
                    });
                }
                catch (Exception ex)
                {
                    log.Critical(ex, "Exception in DOMContentLoaded handler: ");
                }
            };

            screenshottingTask = CaptureScreenshotAsync()
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        log.Critical(t.Exception, "Browser display feed failed");
                });
        }
        catch (Exception ex)
        {
            log.Critical(ex, "Exception in InitializeBrowserPage");
        }
    }

    private void UpdateTitle(string title)
    {
        int fontSize = Title.FontSize;
        string t = string.IsNullOrWhiteSpace(title) ? "Unknown" : title;
        if (!string.IsNullOrWhiteSpace(currentFaviconPath))
            title = $"\\s[favicon{favicoId}]\\!  -  " + title;
        Title.SetText(title);
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

        if (IsClosing)
            return;

        try
        {
            if (faviconTexture.HasValue)
            {
                ray.UnloadTexture(faviconTexture.Value);
                faviconTexture = null;
            }

            var client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync(faviconUrl);
            client.Dispose();
            try
            {
                string ext = Path.GetExtension(faviconUrl) ?? ".png";
                string path = FavIconsPath + $"\\{favicoId}{ext}";
                await File.WriteAllBytesAsync(path, data);
                favIcoUpdateRequired = true;
                currentFaviconPath = path;
            }
            catch
            {
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "Exception in UpdateFaviconAsync: ");
        }
    }

    private void DisposeFavicon()
    {
        if (faviconTexture.HasValue && faviconTexture.Value.Id > 0)
        {
            ray.UnloadTexture(faviconTexture.Value);
            faviconTexture = null;
        }

        if (!string.IsNullOrWhiteSpace(currentFaviconPath) && File.Exists(currentFaviconPath))
        {
            File.Delete(currentFaviconPath);
            currentFaviconPath = null;
        }
    }

    protected override void Update()
    {
        if (page == null) return;

        // Trigger screenshot capture every update interval
        if ((DateTime.Now - lastUpdateTime).TotalSeconds >= updateInterval)
        {
            screenshottingTask ??= CaptureScreenshotAsync(); // async capture
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
            Image img = ray.LoadImageFromMemory(".png", ms.ToArray());

            unsafe
            {
                if (webTexture.HasValue)
                {
                    ray.UpdateTexture(webTexture.Value, img.Data);
                    ray.UnloadImage(img);
                }
                else
                {
                    webTexture = ray.LoadTextureFromImage(img);
                    SpriteElement.Sprite = new(webTexture.Value, true);
                    ray.UnloadImage(img);
                }
            }
        }

        if (favIcoUpdateRequired)
        {
            favIcoUpdateRequired = false;
            var tex = ray.LoadTexture(currentFaviconPath);
            if (tex.Id == 0)
            {
                log.Warning("Favico was badly or never loaded");
                return;
            }
            faviconTexture = tex;
            RichSpriteRegistry.RegisterSprite(
                "favicon" + favicoId,
                new(tex, false)
            );

            UpdateTitle(page.GetTitleAsync().GetAwaiter().GetResult());
        }

        base.Update();
    }

    protected internal override void DrawContent(Rectangle bounds)
    {
        base.DrawContent(bounds);

        if (navigationFailed)
        {
            string msg = $"Invalid link: {failedUrl}";
            int fontSize = 20;
            Vector2 textSize = ray.MeasureTextEx(
                ray.GetFontDefault(),
                msg,
                fontSize,
                1
            );

            float drawX = bounds.X + (bounds.Width - textSize.X) / 2;
            float drawY = bounds.Y + (bounds.Height - textSize.Y) / 3;

            ray.DrawTextEx(
                ray.GetFontDefault(),
                msg,
                new Vector2(drawX, drawY),
                fontSize,
                1,
                new Color(255, 100, 100, (int)(255 * Style.ContentAlpha))
            );

            bounds.Y += (int)(textSize.Y + UIConstants.CONTENT_PADDING);
            return;
        }


        if (!webTexture.HasValue) return;

        HandleInput(bounds);
    }

    public void HandleInput(Rectangle bounds)
    {
        if (page == null || !webTexture.HasValue) return;

        Vector2 mousePos = Input.MousePosition;
        Rectangle texRect = SpriteElement.LastRenderBounds;

        if (mousePos.X >= texRect.X && mousePos.X <= texRect.X + texRect.Width &&
            mousePos.Y >= texRect.Y && mousePos.Y <= texRect.Y + texRect.Height)
        {
            float scaleX = page.Viewport.Width / texRect.Width;
            float scaleY = page.Viewport.Height / texRect.Height;

            float pageX = (mousePos.X - texRect.X) * scaleX;
            float pageY = (mousePos.Y - texRect.Y) * scaleY;

            if (ray.IsMouseButtonPressed(MouseButton.Left))
            {
                _ = page.Mouse.ClickAsync((decimal)pageX, (decimal)pageY)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            log.Warning(t.Exception, "Mouse.ClickAsync failed");
                    });
            }

            float wheel = ray.GetMouseWheelMove();
            if (wheel != 0)
            {
                _ = page.Mouse.WheelAsync(0, (decimal)(-wheel * 50))
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            log.Warning(t.Exception, "Mouse.WheelAsync failed: ");
                    });
            }
        }

        // --- KEYBOARD INPUT ---
        while (ray.GetKeyPressed() is int keyCode && keyCode != 0)
        {
            if (RaylibToPuppeteerKey.TryGetValue((KeyboardKey)keyCode, out string puppeteerKey))
            {
                if (puppeteerKey.Length == 1 &&
                    char.IsLetter(puppeteerKey[0]) &&
                    (ray.IsKeyDown(KeyboardKey.LeftShift) ||
                     ray.IsKeyDown(KeyboardKey.RightShift)))
                {
                    puppeteerKey = puppeteerKey.ToUpper();
                }

                _ = page.Keyboard.PressAsync(puppeteerKey)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            log.Warning(t.Exception, "Exception in InitializeBrowserPage");
                    });
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
            log.Critical(ex, "Exception in InitializeBrowserPage");
        }
        finally
        {
            screenshottingTask = null;
        }
    }

    private static readonly Dictionary<KeyboardKey, string> RaylibToPuppeteerKey = new()
    {
        // Letters A-Z
        { KeyboardKey.A, "a" },
        { KeyboardKey.B, "b" },
        { KeyboardKey.C, "c" },
        { KeyboardKey.D, "d" },
        { KeyboardKey.E, "e" },
        { KeyboardKey.F, "f" },
        { KeyboardKey.G, "g" },
        { KeyboardKey.H, "h" },
        { KeyboardKey.I, "i" },
        { KeyboardKey.J, "j" },
        { KeyboardKey.K, "k" },
        { KeyboardKey.L, "l" },
        { KeyboardKey.M, "m" },
        { KeyboardKey.N, "n" },
        { KeyboardKey.O, "o" },
        { KeyboardKey.P, "p" },
        { KeyboardKey.Q, "q" },
        { KeyboardKey.R, "r" },
        { KeyboardKey.S, "s" },
        { KeyboardKey.T, "t" },
        { KeyboardKey.U, "u" },
        { KeyboardKey.V, "v" },
        { KeyboardKey.W, "w" },
        { KeyboardKey.X, "x" },
        { KeyboardKey.Y, "y" },
        { KeyboardKey.Z, "z" },

        // Control keys
        { KeyboardKey.Enter, "Enter" },
        { KeyboardKey.Backspace, "Backspace" },
        { KeyboardKey.Tab, "Tab" },
        { KeyboardKey.Delete, "Delete" },
        { KeyboardKey.Escape, "Escape" },
        { KeyboardKey.CapsLock, "CapsLock" },
        { KeyboardKey.LeftShift, "Shift" },
        { KeyboardKey.RightShift, "Shift" },
        { KeyboardKey.LeftControl, "Control" },
        { KeyboardKey.RightControl, "Control" },
        { KeyboardKey.LeftAlt, "Alt" },
        { KeyboardKey.RightAlt, "Alt" },

        // Arrow keys
        { KeyboardKey.Up, "ArrowUp" },
        { KeyboardKey.Down, "ArrowDown" },
        { KeyboardKey.Left, "ArrowLeft" },
        { KeyboardKey.Right, "ArrowRight" },

        // Function keys
        { KeyboardKey.F1, "F1" },
        { KeyboardKey.F2, "F2" },
        { KeyboardKey.F3, "F3" },
        { KeyboardKey.F4, "F4" },
        { KeyboardKey.F5, "F5" },
        { KeyboardKey.F6, "F6" },
        { KeyboardKey.F7, "F7" },
        { KeyboardKey.F8, "F8" },
        { KeyboardKey.F9, "F9" },
        { KeyboardKey.F10, "F10" },
        { KeyboardKey.F11, "F11" },
        { KeyboardKey.F12, "F12" },

        // Symbols / punctuation
        { KeyboardKey.Semicolon, ";" },
        { KeyboardKey.Comma, "," },
        { KeyboardKey.Period, "." },
        { KeyboardKey.Slash, "/" },
        { KeyboardKey.Backslash, "\\" },
        { KeyboardKey.Apostrophe, "'" },
        { KeyboardKey.Minus, "-" },
        { KeyboardKey.Equal, "=" },
        { KeyboardKey.LeftBracket, "[" },
        { KeyboardKey.RightBracket, "]" },
        { KeyboardKey.Grave, "`" },

        // Optional: more symbols
        { KeyboardKey.Space, " " },
    };

    public UISpriteContent SpriteElement { get; }

    public override void Close()
    {
        Dispose();
        base.Close();
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        Task.Run(async () =>
        {
            await (page?.CloseAsync() ?? Task.CompletedTask);
            browser?.Dispose();
        });
        DisposeFavicon();
        AppDomain.CurrentDomain.ProcessExit -= PanickedAppClose;
    }
}