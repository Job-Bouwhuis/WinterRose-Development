using WinterRose.FuzzySearching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomTesting.ConsoleWindowing
{
    public static class TerminalFileExplorer
    {
        // Default: start at the root of the current drive (e.g., C:\)
        public static Window Show(int x, int y, int width, int height)
        {
            string root = Path.GetPathRoot(Environment.CurrentDirectory);
            if (string.IsNullOrEmpty(root)) root = Environment.CurrentDirectory;
            return Show(root, x, y, width, height);
        }

        public static Window Show(string rootPath, int x, int y, int width, int height)
        {
            var window = new Window("Terminal File Explorer", x, y, width, height)
            {
                BorderColor = ConsoleColor.DarkCyan,
                TitleColor = ConsoleColor.White
            };

            var root = new LayerStack(Orientation.Vertical) { Spacing = 1 };

            // Path row
            var pathRow = new LayerStack(Orientation.Horizontal) { Spacing = 1 };
            pathRow.AddControl(new Label("Path:"));
            var pathInput = new TextInput(NormalizeRoot(rootPath), Math.Max(20, width - 24));
            pathRow.AddControl(pathInput);
            var openButton = new Button("Open");
            pathRow.AddControl(openButton);
            root.AddChild(pathRow);

            // Search row
            var searchRow = new LayerStack(Orientation.Horizontal) { Spacing = 1 };
            searchRow.AddControl(new Label("Find:"));
            var searchInput = new TextInput("", Math.Max(20, width - 16));
            searchRow.AddControl(searchInput);
            root.AddChild(searchRow);

            // Checkbox for content search
            var contentScan = new Checkbox("Search file contents");
            root.AddControl(contentScan);

            // Tree control
            var tree = new TerminalFileExplorerTree(pathInput.Text, Math.Max(20, width - 4), Math.Max(8, height - 8));
            root.AddControl(tree);

            // Wire up events
            openButton.Click += () =>
            {
                if (tree.TrySetRoot(pathInput.Text))
                    pathInput.Text = tree.RootPath;
            };
            searchInput.TextChanged += text => tree.SearchText = text;
            contentScan.CheckedChanged += enabled => tree.ScanContents = enabled;
            tree.RootChanged += path => pathInput.Text = path;

            window.SetRoot(root);
            window.Open();

            // Force an initial refresh so the tree populates immediately
            tree.MarkDirty();
            tree.Refresh();

            return window;
        }

        private static string NormalizeRoot(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
                return Directory.Exists(fullPath) ? fullPath : Environment.CurrentDirectory;
            }
            catch
            {
                return Environment.CurrentDirectory;
            }
        }
    }

    // ========== FILE EXPLORER TREE ==========
    public class TerminalFileExplorerTree : Control
    {
        private enum EntryKind { Directory, File, ContentMatch }

        private sealed class ExplorerEntry
        {
            public required string Name { get; init; }
            public required string FullPath { get; init; }
            public EntryKind Kind { get; init; }
            public int Depth { get; init; }
            public bool IsExpanded { get; set; }
            public int LineNumber { get; init; }
            public string MatchedLine { get; init; } = "";
        }

        private const int MaxVisibleEntries = 250;
        private const int MaxDirectoryDepth = 6;
        private const int MaxContentFiles = 80;
        private const int MaxFileBytesForScan = 256 * 1024;
        private const float MatchThreshold = 0.18f;

        private readonly HashSet<string> _expandedDirectories = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<ExplorerEntry> _visibleEntries = new();
        private readonly HashSet<string> _textExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".txt", ".md", ".json", ".xml", ".html", ".htm", ".css", ".js", ".ts",
            ".csv", ".log", ".ini", ".config", ".yml", ".yaml", ".ps1", ".bat", ".cmd",
            ".sln", ".csproj", ".props", ".targets", ".gitignore"
        };
        private readonly HashSet<string> _binaryExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".pdb", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico",
            ".zip", ".7z", ".rar", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".bin", ".obj", ".db", ".sqlite", ".mp3", ".mp4", ".mov", ".avi"
        };

        private string _rootPath;
        private string _searchText = "";
        private bool _scanContents;
        private int _selectedIndex;
        private int _scrollOffset;
        private bool _needsRefresh = true;
        private string _status = "";

        public event Action<string> RootChanged;
        public string RootPath => _rootPath;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? "";
                _needsRefresh = true;
                MarkDirty();
            }
        }

        public bool ScanContents
        {
            get => _scanContents;
            set
            {
                _scanContents = value;
                _needsRefresh = true;
                MarkDirty();
            }
        }

        public override bool ConsumesNavigation => IsFocused;

        public TerminalFileExplorerTree(string rootPath, int width, int height)
        {
            _rootPath = NormalizeRoot(rootPath);
            _expandedDirectories.Add(_rootPath);
            Width = width;
            Height = height;
            Focusable = true;
            _needsRefresh = true; // ensure first refresh
        }

        public bool TrySetRoot(string path)
        {
            string normalized = NormalizeRoot(path);
            if (!Directory.Exists(normalized)) return false;
            SetRoot(normalized);
            return true;
        }

        public void Refresh()
        {
            if (!_needsRefresh) return;
            _visibleEntries.Clear();
            if (!Directory.Exists(_rootPath))
            {
                _status = "Root path no longer exists.";
                _needsRefresh = false;
                return;
            }

            BuildDirectory(_rootPath, 0);
            if (!string.IsNullOrWhiteSpace(_searchText) && _scanContents)
                AddContentMatches();

            if (_selectedIndex >= _visibleEntries.Count)
                _selectedIndex = Math.Max(0, _visibleEntries.Count - 1);
            EnsureSelectionVisible();

            _status = $"{_visibleEntries.Count} items";
            if (_scanContents && !string.IsNullOrWhiteSpace(_searchText))
                _status += " with content scan";
            _needsRefresh = false;
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
            Width = Math.Max(10, maxWidth);
            Height = Math.Max(3, maxHeight);
        }

        public override void Draw(int offsetX, int offsetY)
        {
            if (_needsRefresh) Refresh();

            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            Renderer.ClearArea(drawX, drawY, Width, Height, BackColor);

            string rootLine = $"Root: {TrimLeft(_rootPath, Width - 6)}";
            Renderer.DrawString(drawX, drawY, TrimRight(rootLine, Width).PadRight(Width), ConsoleColor.DarkCyan, BackColor);
            DrawEntries(drawX, drawY + 1, Math.Max(0, Height - 2));
            Renderer.DrawString(drawX, drawY + Height - 1, TrimRight(_status, Width).PadRight(Width), ConsoleColor.DarkGray, BackColor);
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W && key.Modifiers == 0)
                MoveSelection(-1);
            else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S && key.Modifiers == 0)
                MoveSelection(1);
            else if (key.Key == ConsoleKey.PageUp)
                MoveSelection(-Math.Max(1, Height - 2));
            else if (key.Key == ConsoleKey.PageDown)
                MoveSelection(Math.Max(1, Height - 2));
            else if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A && key.Modifiers == 0)
                CollapseSelected();
            else if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D && key.Modifiers == 0)
                ExpandSelected();
            else if (key.Key == ConsoleKey.Enter)
                ActivateSelected(key.Modifiers);
            else if (key.Key == ConsoleKey.O)
                OpenSelectedDirectoryAsRoot();
            else if (key.Key == ConsoleKey.Backspace)
                OpenParentAsRoot();

            MarkDirty();
        }

        public override void Activate() => ActivateSelected(ConsoleModifiers.None);

        private void SetRoot(string path)
        {
            _rootPath = path;
            _expandedDirectories.Clear();
            _expandedDirectories.Add(_rootPath);
            _selectedIndex = 0;
            _scrollOffset = 0;
            _needsRefresh = true;
            RootChanged?.Invoke(_rootPath);
            MarkDirty();
        }

        private void MoveSelection(int delta)
        {
            if (_visibleEntries.Count == 0) return;
            _selectedIndex = Math.Clamp(_selectedIndex + delta, 0, _visibleEntries.Count - 1);
            EnsureSelectionVisible();
        }

        private void EnsureSelectionVisible()
        {
            int visibleCount = Math.Max(1, Height - 2);
            if (_selectedIndex < _scrollOffset) _scrollOffset = _selectedIndex;
            if (_selectedIndex >= _scrollOffset + visibleCount)
                _scrollOffset = _selectedIndex - visibleCount + 1;
        }

        private void ActivateSelected(ConsoleModifiers modifiers)
        {
            if (_visibleEntries.Count == 0) return;
            var entry = _visibleEntries[_selectedIndex];
            if (entry.Kind == EntryKind.Directory && modifiers.HasFlag(ConsoleModifiers.Control))
                SetRoot(entry.FullPath);
            else if (entry.Kind == EntryKind.Directory)
                ToggleDirectory(entry.FullPath);
        }

        private void ExpandSelected()
        {
            if (_visibleEntries.Count == 0) return;
            var entry = _visibleEntries[_selectedIndex];
            if (entry.Kind != EntryKind.Directory) return;
            _expandedDirectories.Add(entry.FullPath);
            _needsRefresh = true;
        }

        private void CollapseSelected()
        {
            if (_visibleEntries.Count == 0) return;
            var entry = _visibleEntries[_selectedIndex];
            if (entry.Kind != EntryKind.Directory) return;
            _expandedDirectories.Remove(entry.FullPath);
            _needsRefresh = true;
        }

        private void ToggleDirectory(string path)
        {
            if (!_expandedDirectories.Add(path))
                _expandedDirectories.Remove(path);
            _needsRefresh = true;
        }

        private void OpenSelectedDirectoryAsRoot()
        {
            if (_visibleEntries.Count == 0) return;
            var entry = _visibleEntries[_selectedIndex];
            if (entry.Kind == EntryKind.Directory)
                SetRoot(entry.FullPath);
        }

        private void OpenParentAsRoot()
        {
            var parent = Directory.GetParent(_rootPath);
            if (parent != null)
                SetRoot(parent.FullName);
        }

        private void BuildDirectory(string path, int depth)
        {
            if (_visibleEntries.Count >= MaxVisibleEntries || depth > MaxDirectoryDepth) return;

            IEnumerable<string> directories = Enumerable.Empty<string>();
            IEnumerable<string> files = Enumerable.Empty<string>();
            try
            {
                directories = Directory.EnumerateDirectories(path);
                files = Directory.EnumerateFiles(path);
            }
            catch
            {
                return;
            }

            foreach (var entry in RankPaths(directories, EntryKind.Directory, depth).Concat(RankPaths(files, EntryKind.File, depth)))
            {
                if (_visibleEntries.Count >= MaxVisibleEntries) return;
                _visibleEntries.Add(entry);
                if (entry.Kind == EntryKind.Directory && _expandedDirectories.Contains(entry.FullPath))
                    BuildDirectory(entry.FullPath, depth + 1);
            }
        }

        private IEnumerable<ExplorerEntry> RankPaths(IEnumerable<string> paths, EntryKind kind, int depth)
        {
            string query = _searchText.Trim();
            var materialized = paths.ToList();
            if (string.IsNullOrWhiteSpace(query))
            {
                return materialized
                    .OrderBy(p => Path.GetFileName(p), StringComparer.OrdinalIgnoreCase)
                    .Select(p => ToEntry(p, kind, depth));
            }

            return materialized
                .SearchMany(query, p => Path.GetFileName(p), FuzzyComparisonType.IgnoreCase)
                .Where(x => x.score >= MatchThreshold || kind == EntryKind.Directory)
                .OrderByDescending(x => x.score)
                .ThenBy(x => Path.GetFileName(x.item), StringComparer.OrdinalIgnoreCase)
                .Select(x => ToEntry(x.item, kind, depth));
        }

        private ExplorerEntry ToEntry(string path, EntryKind kind, int depth)
        {
            return new ExplorerEntry
            {
                Name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                FullPath = path,
                Kind = kind,
                Depth = depth,
                IsExpanded = kind == EntryKind.Directory && _expandedDirectories.Contains(path)
            };
        }

        private void AddContentMatches()
        {
            string query = _searchText.Trim();
            int scanned = 0;
            foreach (string file in EnumerateFilesSafely(_rootPath).Take(MaxContentFiles * 4))
            {
                if (scanned >= MaxContentFiles || _visibleEntries.Count >= MaxVisibleEntries) return;
                if (IsLikelyBinary(file)) continue;
                scanned++;

                try
                {
                    var lines = File.ReadLines(file)
                        .Select((line, index) => new { line, number = index + 1 })
                        .SearchMany(query, x => x.line, FuzzyComparisonType.IgnoreCase)
                        .Where(x => x.score >= MatchThreshold)
                        .Take(2);

                    foreach (var match in lines)
                    {
                        _visibleEntries.Add(new ExplorerEntry
                        {
                            Name = Path.GetFileName(file),
                            FullPath = file,
                            Kind = EntryKind.ContentMatch,
                            Depth = 1,
                            LineNumber = match.item.number,
                            MatchedLine = match.item.line.Trim()
                        });
                    }
                }
                catch
                {
                }
            }
        }

        private IEnumerable<string> EnumerateFilesSafely(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                IEnumerable<string> dirs = Enumerable.Empty<string>();
                IEnumerable<string> files = Enumerable.Empty<string>();
                try
                {
                    dirs = Directory.EnumerateDirectories(current);
                    files = Directory.EnumerateFiles(current);
                }
                catch
                {
                }

                foreach (var file in files)
                    yield return file;
                foreach (var dir in dirs)
                    pending.Push(dir);
            }
        }

        private bool IsLikelyBinary(string file)
        {
            try
            {
                var info = new FileInfo(file);
                if (info.Length > MaxFileBytesForScan) return true;
                string ext = info.Extension;
                if (_binaryExtensions.Contains(ext)) return true;
                if (_textExtensions.Contains(ext)) return false;

                using var stream = File.OpenRead(file);
                Span<byte> buffer = stackalloc byte[512];
                int read = stream.Read(buffer);
                for (int i = 0; i < read; i++)
                {
                    byte b = buffer[i];
                    if (b == 0) return true;
                    if (b < 8 || b > 13 && b < 32) return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        private void DrawEntries(int x, int y, int height)
        {
            for (int row = 0; row < height; row++)
            {
                int index = _scrollOffset + row;
                string line = "";
                ConsoleColor fg = ConsoleColor.Gray;
                ConsoleColor bg = BackColor;

                if (index < _visibleEntries.Count)
                {
                    var entry = _visibleEntries[index];
                    bool selected = IsFocused && index == _selectedIndex;
                    bg = selected ? ConsoleColor.DarkBlue : BackColor;
                    fg = selected ? ConsoleColor.White : EntryColor(entry);
                    line = FormatEntry(entry);
                }

                Renderer.DrawString(x, y + row, TrimRight(line, Width).PadRight(Width), fg, bg);
            }
        }

        private static ConsoleColor EntryColor(ExplorerEntry entry)
        {
            return entry.Kind switch
            {
                EntryKind.Directory => ConsoleColor.Cyan,
                EntryKind.ContentMatch => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
        }

        private string FormatEntry(ExplorerEntry entry)
        {
            string indent = new string(' ', Math.Min(entry.Depth, 20) * 2);
            if (entry.Kind == EntryKind.Directory)
            {
                string marker = entry.IsExpanded ? "[-]" : "[+]";
                return $"{indent}{marker} {entry.Name}";
            }
            if (entry.Kind == EntryKind.ContentMatch)
            {
                string line = TrimRight(entry.MatchedLine, Math.Max(10, Width - indent.Length - entry.Name.Length - 12));
                return $"{indent}~ {entry.Name}:{entry.LineNumber} {line}";
            }
            return $"{indent}- {entry.Name}";
        }

        private static string NormalizeRoot(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
                return Directory.Exists(fullPath) ? fullPath : Environment.CurrentDirectory;
            }
            catch
            {
                return Environment.CurrentDirectory;
            }
        }

        private static string TrimRight(string text, int width)
        {
            if (width <= 0) return "";
            return text.Length <= width ? text : text[..width];
        }

        private static string TrimLeft(string text, int width)
        {
            if (width <= 0) return "";
            if (text.Length <= width) return text;
            return "..." + text[^Math.Max(0, width - 3)..];
        }
    }
}