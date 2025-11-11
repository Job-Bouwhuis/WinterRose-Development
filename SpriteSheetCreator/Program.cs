using EnvDTE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WinterRose.ForgeSignal;
using WinterRose.ForgeThread;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Worlds;

internal class Program : Application
{
    string[] args;
    Toast processToast;
    UIText text;
    UIProgress progressBar;
    UIText currentFileName;
    ThreadLoom loom;
    bool processingStarted = false;
    CoroutineHandle processingEnumerator;
    private bool calledClose;

    SplitResult splitResult;
    private bool cancelled;
    private bool cancelToastShown;

    static readonly string[] ALLOWED_EXTENSIONS = new[] { ".png", ".jpg", ".jpeg", ".bmp" };

    public Program() : base(false, true)
    {
        processToast = new Toast(ToastType.Info, ToastRegion.Left, ToastStackSide.Top);
        progressBar = new UIProgress();
        text = new UIText("", UIFontSizePreset.Title);
        currentFileName = new UIText("", UIFontSizePreset.Subtext);
        processToast.AddContent(text);
        processToast.AddContent(progressBar);
        loom = new ThreadLoom();
        loom.RegisterWorkerThread("Processor", ThreadPriority.Highest, true, 24);
    }

    public void SplitDialog()
    {
        Dialog d = new("Enter details", DialogPlacement.CenterSmall, DialogPriority.High);

        UINumericUpDown<int> width;
        UINumericUpDown<int> height;
        UIValueSlider<int> padX;
        UIValueSlider<int> padY;

        {
            UIColumns sizeCols = new UIColumns();

            UIColumns widthCols = new UIColumns();
            widthCols.AddToColumn(0, new UIText("width"));
            UINumericUpDown<int> widthUpDown = width = new UINumericUpDown<int>(8, 1024, 256);
            widthUpDown.OnValueChangedBasic.Subscribe(Invocation.Create((int i) =>
            {
                d.DialogResult ??= new SplitResult();

                SplitResult r = d.DialogResult as SplitResult;

                r.width = i;
            }));
            widthCols.AddToColumn(1, widthUpDown);
            sizeCols.AddToColumn(0, widthCols);

            UIColumns heightCols = new UIColumns();
            widthCols.AddToColumn(0, new UIText("width"));
            UINumericUpDown<int> heightUpDown = height = new UINumericUpDown<int>(8, 1024, 256);
            heightUpDown.OnValueChangedBasic.Subscribe(Invocation.Create((int i) =>
            {
                d.DialogResult ??= new SplitResult();

                SplitResult r = d.DialogResult as SplitResult;

                r.height = i;
            }));
            widthCols.AddToColumn(1, heightUpDown);
            sizeCols.AddToColumn(1, heightCols);

            d.AddContent(sizeCols);
        }

        {
            UIColumns paddingCols = new UIColumns();

            UIColumns widthCols = new UIColumns();
            widthCols.AddToColumn(0, new UIText("Padding X"));
            UIValueSlider<int> widthUpDown = padX = new UIValueSlider<int>(0, 16, 0);
            widthUpDown.OnValueChangedBasic.Subscribe(Invocation.Create((int i) =>
            {
                d.DialogResult ??= new SplitResult();

                SplitResult r = d.DialogResult as SplitResult;

                r.paddingX = i;
            }));
            widthCols.AddToColumn(1, widthUpDown);
            paddingCols.AddToColumn(0, widthCols);

            UIColumns heightCols = new UIColumns();
            widthCols.AddToColumn(0, new UIText("Padding Y"));
            UIValueSlider<int> heightUpDown = padY = new UIValueSlider<int>(0, 16, 0);
            heightUpDown.OnValueChangedBasic.Subscribe(Invocation.Create((int i) =>
            {
                d.DialogResult ??= new SplitResult();

                SplitResult r = d.DialogResult as SplitResult;

                r.paddingY = i;
            }));
            widthCols.AddToColumn(1, heightUpDown);
            paddingCols.AddToColumn(1, heightCols);

            d.AddContent(paddingCols);
        }

        UIColumns buttons = new();
        buttons.AddToColumn(0, new UIButton("Submit", (c, b) =>
        {
            SplitResult r = new();
            r.width = width.Value;
            r.height = height.Value;
            r.paddingX = padX.Value;
            r.paddingY = padY.Value;
            ((Dialog)c).DialogResult = r;
            c.Close();
        }));
        buttons.AddToColumn(1, new UIButton("Cacnel", (c, b) =>
        {
            ((Dialog)c).DialogResult = null;
            c.Close();
        }));
        d.AddContent(buttons);
        d.Show();

        d.OnResult += (Dialog dlg, object result) =>
        {
            if (result is SplitResult split)
            {
                splitResult = split;
                processingStarted = true;
                processingEnumerator = loom.InvokeOn("Processor", SplitSingleImageCoroutine(split).GetEnumerator());
            }
        };
    }

    private class SplitResult
    {
        public int width;
        public int height;
        public int paddingX;
        public int paddingY;
    }

    private static void Main(string[] args)
    {
        var p = new Program();
        if (!WinterRose.Windows.OpenFiles(out p.args, "Select all the files you wish to compact", "(*.png)|*.png|(*.jpg)|*.jpg|(*.bmp)|*.bmp|"))
        {
            
            p.cancelled = true;
        }

        // Otherwise, run the overlay normally
        p.RunAsOverlay("SpriteSheet Converter");
    }

    public override World CreateFirstWorld()
    {
        Raylib_cs.Raylib.SetTargetFPS(144);

        World world = new("");

        return world;
    }

    public override void Update()
    {
        if(cancelled && !cancelToastShown)
        {
            Toasts.Warning("Canceled operation!", ToastRegion.Left);
            cancelToastShown = true;
        }
        if (cancelled || processingStarted && processingEnumerator is not null && processingEnumerator.IsComplete)
        {
            if (!calledClose)
            {
                calledClose = true;
                processToast.Close();
                if(processingEnumerator is not null && processingEnumerator.IsComplete)
                    Toasts.Success("Operation completed!", ToastRegion.Left);
            }

            if (Toasts.GetNumberOfToastsActive() == 0)
            {
                Close();
            }
            return;
        }

        if (!processingStarted)
        {
            processingStarted = true;
            processToast.Show();

            if (args != null && args.Length == 1)
            {
                // Single image selected → ask for split details first
                SplitDialog();
            }
            else
            {
                processingEnumerator = loom.InvokeOn("Processor", ProcessFilesCoroutine().GetEnumerator());
            }
        }
    }

    // --- New: file processing & spritesheet generation ---
    private IEnumerable<object> ProcessFilesCoroutine()
    {
        if (args == null || args.Length == 0)
        {
            ShowToast(ToastType.Info, "No files provided.");
            yield return new End();
        }

        var providedFiles = args.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        var existingFiles = providedFiles.Where(File.Exists).ToArray();

        if (existingFiles.Length == 0)
        {
            ShowToast(ToastType.Fatal, "No valid files were passed (no existing files).");
            yield return new End();
        }

        var imageFiles = new List<string>();
        foreach (var file in existingFiles)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!ALLOWED_EXTENSIONS.Contains(ext))
            {
                ShowToast(ToastType.Fatal, $"Not an image file: {Path.GetFileName(file)}");
                // give the UI a frame to process the toast
                yield return null;
            }
            else
            {
                imageFiles.Add(file);
            }
        }

        if (imageFiles.Count == 0)
        {
            ShowToast(ToastType.Fatal, "No image files to process.");
            yield return new End();
        }

        var bitmaps = new List<(Bitmap Bitmap, string Path)>();
        int index = 0;
        int total = imageFiles.Count;
        foreach (var file in imageFiles)
        {
            UpdateProgress(index, total, Path.GetFileName(file));
            try
            {
                var bmp = new Bitmap(file);
                bitmaps.Add((bmp, file));
            }
            catch (Exception)
            {
                ShowToast(ToastType.Fatal, $"Failed to load image: {Path.GetFileName(file)}");
            }

            index++;
            // yield so the UI can update per-file
            yield return null;
        }

        if (bitmaps.Count == 0)
        {
            ShowToast(ToastType.Fatal, "No images successfully loaded.");
            yield return new End();
        }

        var sizeGroups = bitmaps
            .GroupBy(b => (b.Bitmap.Width, b.Bitmap.Height))
            .OrderByDescending(g => g.Count())
            .ToArray();

        var majorityPair = sizeGroups.First().Key;
        int majorityWidth = majorityPair.Width;
        int majorityHeight = majorityPair.Height;

        foreach (var tuple in bitmaps)
        {
            var bmp = tuple.Bitmap;
            var path = tuple.Path;
            if (bmp.Width != majorityWidth || bmp.Height != majorityHeight)
            {
                ShowToast(ToastType.Warning, $"Different size: {Path.GetFileName(path)} ({bmp.Width}x{bmp.Height})");
                // allow a frame to show the warning
                yield return null;
            }
        }

        // --- positional detection start ---
        // mapping for 3x3 (row-major): TopLeft(0), TopMid(1), TopRight(2),
        // MidLeft(3), Mid(4), MidRight(5), BottomLeft(6), BottomMid(7), BottomRight(8)
        int? DetectPositionIndex(string fileNameNoExt)
        {
            var s = fileNameNoExt.ToLowerInvariant();

            // check combined tokens first (order matters so 'midright' resolves before 'mid')
            var checks = new (string token, int idx)[]
            {
            ("topleft", 0), ("topcenter", 1), ("topcentre", 1), ("topmid", 1), ("topmiddle", 1), ("topright", 2),
            ("midleft", 3), ("midright", 5),
            ("bottomleft", 6), ("bottomcenter", 7), ("bottomcentre", 7), ("bottommid", 7), ("bottommiddle", 7), ("bottomright", 8),
            // single-center tokens
            ("mid", 4), ("center", 4), ("centre", 4), ("middle", 4)
            };

            foreach (var (token, idx) in checks)
            {
                if (s.Contains(token))
                    return idx;
            }

            return null;
        }

        // assign matched positions into a map; unassigned go into a list
        var positionedMap = new Dictionary<int, (Bitmap Bitmap, string Path)>();
        var unpositioned = new List<(Bitmap Bitmap, string Path)>();
        var unpositionedNames = new List<string>();
        int matchedCount = 0;

        foreach (var tuple in bitmaps)
        {
            var nameNoExt = Path.GetFileNameWithoutExtension(tuple.Path);
            var pos = DetectPositionIndex(nameNoExt);
            if (pos.HasValue)
            {
                if (!positionedMap.ContainsKey(pos.Value))
                {
                    positionedMap[pos.Value] = tuple;
                    matchedCount++;
                }
                else
                {
                    // collision: two files claim same cell; keep the first, append this to unpositioned
                    unpositioned.Add(tuple);
                    unpositionedNames.Add(Path.GetFileName(tuple.Path));
                }
            }
            else
            {
                unpositioned.Add(tuple);
                unpositionedNames.Add(Path.GetFileName(tuple.Path));
            }
        }

        if (matchedCount == 0)
        {
            // none matched — warn and fall back to original ordering behavior
            ShowToast(ToastType.Warning, "No positional tokens found — generating spritesheet in provided order.");
            yield return null;

            // fall back: simple grid using the original order
            var fallbackList = bitmaps.ToList();

            int countFb = fallbackList.Count;
            int columnsFb = (int)Math.Ceiling(Math.Sqrt(countFb));
            int rowsFb = (int)Math.Ceiling((double)countFb / columnsFb);

            int atlasWidthFb = columnsFb * majorityWidth;
            int atlasHeightFb = rowsFb * majorityHeight;

            using (var atlas = new Bitmap(atlasWidthFb, atlasHeightFb))
            using (var g = Graphics.FromImage(atlas))
            {
                g.Clear(Color.Transparent);

                for (int i = 0; i < fallbackList.Count; i++)
                {
                    var bmp = fallbackList[i].Bitmap;
                    var path = fallbackList[i].Path;

                    UpdateProgress(i + 1, countFb, Path.GetFileName(path));

                    int row = i / columnsFb;
                    int col = i % columnsFb;

                    int cellX = col * majorityWidth;
                    int cellY = row * majorityHeight;

                    int drawX = cellX + (majorityWidth - bmp.Width) / 2;
                    int drawY = cellY + (majorityHeight - bmp.Height) / 2;

                    g.DrawImage(bmp, drawX, drawY, bmp.Width, bmp.Height);
                    yield return null;
                }

                var firstFolderFb = Path.GetDirectoryName(fallbackList[0].Path) ?? Environment.CurrentDirectory;
                string baseNameFb = "spritesheet";
                string outPathFb = Path.Combine(firstFolderFb, baseNameFb + ".png");
                int suffixFb = 1;
                while (File.Exists(outPathFb))
                    outPathFb = Path.Combine(firstFolderFb, baseNameFb + $"_{suffixFb++}.png");

                atlas.Save(outPathFb, System.Drawing.Imaging.ImageFormat.Png);
                ShowToast(ToastType.Info, $"Spritesheet saved: {Path.GetFileName(outPathFb)}");
                yield return null;
            }

            // cleanup
            foreach (var t in fallbackList)
            {
                t.Bitmap.Dispose();
                yield return null;
            }

            UpdateProgress(0, 0, string.Empty);
            yield return new End();
        }

        // warn about some unpositioned files, if any
        if (unpositioned.Count > 0)
        {
            var listPreview = string.Join(", ", unpositionedNames.Take(6));
            var more = unpositionedNames.Count > 6 ? $" (+{unpositionedNames.Count - 6} more)" : "";
            ShowToast(ToastType.Warning, $"Some files lacked positional tokens and will be appended: {listPreview}{more}");
            yield return null;
        }

        // Now compute final cell assignments: fill empty cells with unpositioned in sequence
        int columns = 3;
        int nextIndex = 0;
        foreach (var tup in unpositioned)
        {
            while (positionedMap.ContainsKey(nextIndex))
                nextIndex++;
            positionedMap[nextIndex] = tup;
            nextIndex++;
        }

        int totalAssigned = positionedMap.Keys.Max() + 1;
        int rows = (int)Math.Ceiling((double)totalAssigned / columns);

        int atlasWidth = columns * majorityWidth;
        int atlasHeight = rows * majorityHeight;

        // Draw by iterating cells 0..totalAssigned-1; empty cells remain transparent
        using (var atlas = new Bitmap(atlasWidth, atlasHeight))
        using (var g = Graphics.FromImage(atlas))
        {
            g.Clear(Color.Transparent);

            for (int i = 0; i < totalAssigned; i++)
            {
                if (positionedMap.TryGetValue(i, out var tup))
                {
                    var bmp = tup.Bitmap;
                    var path = tup.Path;

                    UpdateProgress(i + 1, totalAssigned, Path.GetFileName(path));

                    int row = i / columns;
                    int col = i % columns;

                    int cellX = col * majorityWidth;
                    int cellY = row * majorityHeight;

                    int drawX = cellX + (majorityWidth - bmp.Width) / 2;
                    int drawY = cellY + (majorityHeight - bmp.Height) / 2;

                    g.DrawImage(bmp, drawX, drawY, bmp.Width, bmp.Height);
                }

                // give the renderer a frame after each cell
                yield return null;
            }

            var firstFolder = Path.GetDirectoryName(bitmaps[0].Path) ?? Environment.CurrentDirectory;
            string baseName = "spritesheet";
            string outPath = Path.Combine(firstFolder, baseName + ".png");
            int suffix = 1;
            while (File.Exists(outPath))
            {
                outPath = Path.Combine(firstFolder, baseName + $"_{suffix++}.png");
            }

            atlas.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
            ShowToast(ToastType.Info, $"Spritesheet saved: {Path.GetFileName(outPath)}");
            // let the UI show the saved toast
            yield return null;
        }

        // dispose bitmaps
        foreach (var tuple in bitmaps)
        {
            tuple.Bitmap.Dispose();
            // allow a frame while cleaning up
            yield return null;
        }

        UpdateProgress(0, 0, string.Empty);
        yield return new End();
    }

    void UpdateProgress(int current, int total, string fileName)
    {
        // Update UI fields if available; safe-guard against nulls
        if (progressBar != null && total > 0)
        {
            progressBar.SetProgress((float)current / total);
        }

        if (text != null && !string.IsNullOrEmpty(fileName))
        {
            text.Text = $"Processing: {fileName} ({current}/{total})";
        }
        else if (text != null)
        {
            text.Text = string.Empty;
        }
    }

    void ShowToast(ToastType type, string message)
    {
        try
        {
            if (processToast != null)
            {
                processToast.AddContent(new UIText(message, UIFontSizePreset.Subtext));
            }
        }
        catch
        {
            Console.WriteLine($"[{type}] {message}");
        }
    }

    private IEnumerable<object> SplitSingleImageCoroutine(SplitResult split)
    {
        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            ShowToast(ToastType.Fatal, "Selected file no longer exists!");
            yield return new End();
        }

        Bitmap source;
        try
        {
            source = new Bitmap(filePath);
        }
        catch (Exception ex)
        {
            ShowToast(ToastType.Fatal, $"Failed to load image: {ex.Message}");
            yield break;
        }

        int tileW = split.width;
        int tileH = split.height;
        int padX = split.paddingX;
        int padY = split.paddingY;

        if (tileW <= 0 || tileH <= 0)
        {
            ShowToast(ToastType.Fatal, "Invalid tile size.");
            source.Dispose();
            yield return new End();
        }

        int tilesX = (int)Math.Floor((float)(source.Width + padX) / (tileW + padX));
        int tilesY = (int)Math.Floor((float)(source.Height + padY) / (tileH + padY));

        var folder = Path.Combine(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory,
                                  Path.GetFileNameWithoutExtension(filePath) + "_tiles");
        Directory.CreateDirectory(folder);

        int total = tilesX * tilesY;
        int index = 0;

        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                int srcX = x * (tileW + padX);
                int srcY = y * (tileH + padY);
                if (srcX + tileW > source.Width || srcY + tileH > source.Height)
                    continue;

                var rect = new Rectangle(srcX, srcY, tileW, tileH);
                using (var tile = source.Clone(rect, source.PixelFormat))
                {
                    string outPath = Path.Combine(folder, $"tile_{y}_{x}.png");
                    tile.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                index++;
                UpdateProgress(index, total, $"tile_{y}_{x}.png");
                yield return null;
            }
        }

        source.Dispose();
        ShowToast(ToastType.Info, $"Split complete: {index} tiles saved.");
        yield return null;

        try
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
        catch { }

        UpdateProgress(0, 0, "");
        yield return new End();
    }

    private class End
    {
        public End()
        {
        }
    }
}