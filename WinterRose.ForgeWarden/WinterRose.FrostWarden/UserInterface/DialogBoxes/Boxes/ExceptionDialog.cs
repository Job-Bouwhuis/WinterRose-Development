using Raylib_cs;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;

internal class ExceptionDialog : Dialog
{
    public ExceptionDialog(Exception ex, ExceptionDispatchInfo info, RenderTexture2D? lastRenderedFrame)
        : base($"\\c[red]A fatal error occured of type \\c[yellow]'{ex.GetType().Name}'\\c[red] and the game did not handle it",
            $"\\c[#FFAAAA]{ex.Message}\n\n\\c[white]StackTrace:\n{ex.StackTrace}",
            DialogPlacement.CenterSmall,
            DialogPriority.EngineNotifications)
    {
        UIColumns cols = new();
        cols.AddToColumn(0, new UIButton("Ok, and close game", (c, b) =>
        {
            c.Close();
        }));
        cols.AddToColumn(1, new UIButton("Copy error details", (container, button) =>
        {
            string details = $"Error type: {ex.GetType().FullName}\nMessage: {ex.Message}\nStackTrace:\n{ex.StackTrace}";
            Raylib.SetClipboardText(details);
        }));
        cols.AddToColumn(2, new UIButton("Generate crash report", (container, button) =>
        {
            try
            {
                string exeFolder = AppContext.BaseDirectory;
                string timestamp = DateTime.Now.ToString("yyyy_MM_dd___HH_mm_ss");
                string zipFolder = Path.Combine(exeFolder, "fatal errors");
                Directory.CreateDirectory(zipFolder);
                string zipFileName = Path.Combine(zipFolder, $"{timestamp}.zip");

                using (var zip = new System.IO.Compression.ZipArchive(
                           File.Create(zipFileName),
                           System.IO.Compression.ZipArchiveMode.Create))
                {
                    // --- 1. Markdown file ---
                    var mdEntry = zip.CreateEntry("CrashReport.md");
                    using (var writer = new StreamWriter(mdEntry.Open()))
                    {
                        writer.WriteLine($"# Crash Report - {timestamp}");
                        writer.WriteLine();

                        void WriteExceptionMarkdown(Exception exception, int depth = 0, int? index = null)
                        {
                            // unwrap TargetInvocationException if encountered
                            if (exception.GetType() == typeof(TargetInvocationException) && exception.InnerException != null)
                                exception = exception.InnerException;

                            string indent = new string(' ', depth * 4);
                            string header = index.HasValue ? $"[{index}] {exception.GetType().FullName}" : exception.GetType().FullName;

                            writer.WriteLine($"{indent}## {header}");
                            writer.WriteLine();
                            writer.WriteLine($"{indent}**Message:** {exception.Message}");
                            writer.WriteLine();
                            writer.WriteLine($"{indent}**StackTrace:**");
                            writer.WriteLine($"{indent}```");
                            foreach (string line in exception.StackTrace?.Split('\n') ?? Array.Empty<string>())
                                writer.WriteLine($"{indent}{line}");
                            writer.WriteLine($"{indent}```");
                            writer.WriteLine();

                            if (exception is AggregateException aggregateException)
                            {
                                int innerIndex = 0;
                                foreach (var inner in aggregateException.InnerExceptions)
                                {
                                    WriteExceptionMarkdown(inner, depth + 1, innerIndex);
                                    innerIndex++;
                                }
                            }
                            else if (exception.InnerException != null)
                            {
                                WriteExceptionMarkdown(exception.InnerException, depth + 1);
                            }
                        }

                        WriteExceptionMarkdown(ex);
                    }

                    // --- 2. Last rendered frame ---
                    if (lastRenderedFrame?.Texture.Id != 0)
                    {
                        var frameEntry = zip.CreateEntry("LastFrame.png");
                        using var ms = new MemoryStream();
                        Image image = Raylib.LoadImageFromTexture(lastRenderedFrame!.Value.Texture);
                        Raylib.ExportImage(image, "tmp.png");
                        Raylib.UnloadImage(image);

                        using (var fileStream = new TempFile())
                        using (var entryStream = frameEntry.Open())
                            fileStream.CopyTo(entryStream);

                    }

                    string logFolder = Path.Combine(exeFolder, "Logs");
                    string latestLogCandidate = Path.Combine(logFolder, "latest Log.log");

                    string actualLogPath = latestLogCandidate;

                    // check if a .lnk shortcut exists instead
                    string lnkCandidate = Path.ChangeExtension(latestLogCandidate, ".lnk");
                    if (File.Exists(lnkCandidate))
                    {
                        try
                        {
                            // resolve the shortcut using Windows Script Host
                            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                            dynamic shell = Activator.CreateInstance(shellType);
                            dynamic shortcut = shell.CreateShortcut(lnkCandidate);
                            string target = shortcut.TargetPath;
                            if (File.Exists(target))
                                actualLogPath = target;
                        }
                        catch
                        {
                            // fallback: just zip the shortcut itself if resolution fails
                            actualLogPath = lnkCandidate;
                        }
                    }

                    // now check for symlink (reparse point) and resolve if needed
                    FileInfo logFileInfo = new(actualLogPath);
                    if ((logFileInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        // get the absolute target path
                        actualLogPath = Path.GetFullPath(actualLogPath);
                    }

                    // before including the log, flush any pending entries to ensure it's up to date
                    Log.Flush();

                    // finally, include the resolved log in the zip
                    if (File.Exists(actualLogPath))
                    {
                        zip.CreateEntryFromFile(actualLogPath, Path.GetFileName(actualLogPath));
                    }

                }

                // copy path to clipboard for convenience
                Raylib.SetClipboardText(zipFileName);
            }
            catch (Exception zipEx)
            {
                Raylib.SetClipboardText($"Failed to create crash report: {zipEx}");
            }
        }));

        if (Debugger.IsAttached)
            cols.AddToColumn(4, new UIButton("Throw exception", (container, button) =>
            {
                ForgeWardenEngine.Current.AllowThrow = true;
                info.Throw();
            }));


        if (ex is AggregateException aggregateException)
        {
            UITreeNode aggregateNode = new("Aggregate exceptions");

            int exceptionIndex = 0;
            foreach (Exception innerException in aggregateException.InnerExceptions)
            {
                BuildExceptionNode(aggregateNode, innerException, ref exceptionIndex);
                exceptionIndex++;
            }

            AddContent(aggregateNode);
        }
        AddContent(cols);
    }

    void BuildExceptionNode(UITreeNode parentNode, Exception exception, ref int index)
    {
        if (exception.GetType() == typeof(TargetInvocationException) && exception.InnerException != null)
            exception = exception.InnerException;

        UITreeNode exceptionNode = new($"[{index}] {exception.GetType().Name}");
        exceptionNode.AddChild(new UIText($"\\c[#FFAAAA]{exception.Message}"));

        if (!string.IsNullOrWhiteSpace(exception.StackTrace))
        {
            exceptionNode.AddChild(new UIText(
                $"\\c[white]StackTrace:\n{exception.StackTrace}"
            ));
        }

        if (exception is AggregateException aggregateException)
        {
            int innerIndex = 0;
            foreach (Exception inner in aggregateException.InnerExceptions)
            {
                BuildExceptionNode(exceptionNode, inner, ref innerIndex);
                innerIndex++;
            }
        }

        parentNode.AddChild(exceptionNode);
    }
}