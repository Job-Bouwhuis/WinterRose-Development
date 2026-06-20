using System;
using System.Collections.Generic;
using System.Linq;
using WinterRose.FuzzySearching;

namespace RandomTesting.WebsitePreviewFetcher
{
    public class DropdownItem
    {
        public string Text { get; set; }
        public object Context { get; set; }
        public DropdownItem(string text, object context = null)
        {
            Text = text;
            Context = context;
        }
        public override string ToString() => Text;
    }

    public class Dropdown : Control
    {
        private List<DropdownItem> _items = new List<DropdownItem>();
        public IList<DropdownItem> Items => _items;

        public int SelectedIndex { get; private set; } = -1;
        public string SelectedItem => SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex].Text : "";
        public object SelectedContext => SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex].Context : null;
        public event Action<int> SelectionChanged;

        public int MaxFilterResults { get; set; } = 10;

        private bool _isOpen = false;
        private int _filteredIndex = -1;
        private string _filterText = "";
        private List<int> _filteredIndices = new List<int>();
        private int _scrollOffset = 0;
        private const int MaxDropdownItems = 10;

        public Dropdown(int width, IEnumerable<string> items = null)
        {
            Width = width;
            Height = 1;
            if (items != null)
            {
                foreach (var s in items)
                    _items.Add(new DropdownItem(s));
            }
            Focusable = true;
            if (_items.Count > 0) SelectedIndex = 0;
            UpdateFilter();
        }

        public void AddItem(string text, object context = null)
        {
            _items.Add(new DropdownItem(text, context));
            if (SelectedIndex < 0) SelectedIndex = 0;
            UpdateFilter();
            MarkDirty();
        }

        private void UpdateFilter()
        {
            _filteredIndices.Clear();

            if (string.IsNullOrWhiteSpace(_filterText))
            {
                var all = Enumerable.Range(0, _items.Count).ToList();
                _filteredIndices.AddRange(all.Take(MaxFilterResults));
            }
            else
            {
                var results = _items
                    .Select((item, idx) => new { item.Text, Index = idx })
                    .SearchMany(_filterText, item => item.Text, FuzzyComparisonType.IgnoreCase)
                    .Select(r => r.item.Index)
                    .Take(MaxFilterResults)
                    .ToList();

                _filteredIndices.AddRange(results);
            }

            _filteredIndex = _filteredIndices.Count > 0 ? 0 : -1;
            if (_scrollOffset >= _filteredIndices.Count)
                _scrollOffset = Math.Max(0, _filteredIndices.Count - 1);
        }

        public override bool ConsumesNavigation => _isOpen;

        public override void Layout(int maxWidth, int maxHeight)
        {
            // fixed width/height
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            ConsoleColor bg = IsFocused ? ConsoleColor.DarkBlue : BackColor;
            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, bg);

            string display;
            if (_isOpen)
            {
                display = _filterText.PadRight(Width);
                if (display.Length > Width) display = display[..Width];
                Renderer.DrawString(drawX, drawY, display, ForeColor, bg);
                if (Screen.IsCursorVisible())
                {
                    int cursorX = drawX + Math.Min(_filterText.Length, Width - 1);
                    Renderer.DrawChar(cursorX, drawY, '_', ConsoleColor.White, bg);
                }
            }
            else
            {
                display = SelectedItem;
                if (display.Length > Width) display = display[..Width];
                Renderer.DrawString(drawX, drawY, display.PadRight(Width), ForeColor, bg);
            }

            if (_isOpen && Window != null)
            {
                int listTop = drawY + 1;
                int listHeight = Math.Min(_filteredIndices.Count, MaxDropdownItems);
                // Ensure list fits within window
                int maxListBottom = Window.Y + Window.Height - 1;
                if (listTop + listHeight > maxListBottom)
                {
                    listTop = drawY - listHeight;
                    if (listTop < Window.Y + 1) listTop = Window.Y + 1;
                }
                int listWidth = Width;

                for (int i = 0; i < listHeight; i++)
                {
                    int idx = i + _scrollOffset;
                    if (idx >= _filteredIndices.Count) break;
                    int itemIdx = _filteredIndices[idx];
                    string item = _items[itemIdx].Text;
                    if (item.Length > listWidth) item = item[..listWidth];
                    int xPos = drawX;
                    int yPos = listTop + i;
                    ConsoleColor bgItem = (idx == _filteredIndex) ? ConsoleColor.DarkGray : ConsoleColor.DarkBlue;
                    Renderer.DrawString(xPos, yPos, item.PadRight(listWidth), ConsoleColor.White, bgItem);
                }
            }
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (!Enabled) return;
            if (_isOpen)
            {
                HandleDropdownKey(key);
                return;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                Activate();
                return;
            }
            if (key.KeyChar >= 32 && key.KeyChar < 127)
            {
                _filterText += key.KeyChar;
                UpdateFilter();
                if (_filteredIndices.Count > 0)
                {
                    SelectedIndex = _filteredIndices[0];
                    SelectionChanged?.Invoke(SelectedIndex);
                    MarkDirty();
                }
                return;
            }
            if (key.Key == ConsoleKey.Backspace && _filterText.Length > 0)
            {
                _filterText = _filterText[..^1];
                UpdateFilter();
                if (_filteredIndices.Count > 0)
                {
                    SelectedIndex = _filteredIndices[0];
                    SelectionChanged?.Invoke(SelectedIndex);
                    MarkDirty();
                }
                return;
            }
        }

        private void HandleDropdownKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Escape)
            {
                Cancel();
                return;
            }
            if (key.Key == ConsoleKey.Enter)
            {
                if (_filteredIndex >= 0 && _filteredIndex < _filteredIndices.Count)
                {
                    SelectedIndex = _filteredIndices[_filteredIndex];
                    SelectionChanged?.Invoke(SelectedIndex);
                }
                CloseDropdown();
                return;
            }
            if (key.Key == ConsoleKey.UpArrow)
            {
                if (_filteredIndex > 0)
                {
                    _filteredIndex--;
                    if (_filteredIndex < _scrollOffset) _scrollOffset = _filteredIndex;
                }
                else
                {
                    _filteredIndex = _filteredIndices.Count - 1;
                    _scrollOffset = Math.Max(0, _filteredIndex - MaxDropdownItems + 1);
                }
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.DownArrow)
            {
                if (_filteredIndex < _filteredIndices.Count - 1)
                {
                    _filteredIndex++;
                    if (_filteredIndex >= _scrollOffset + MaxDropdownItems)
                        _scrollOffset = _filteredIndex - MaxDropdownItems + 1;
                }
                else
                {
                    _filteredIndex = 0;
                    _scrollOffset = 0;
                }
                MarkDirty();
                return;
            }
            if (key.KeyChar >= 32 && key.KeyChar < 127)
            {
                _filterText += key.KeyChar;
                UpdateFilter();
                if (_filteredIndices.Count > 0)
                {
                    _filteredIndex = 0;
                    _scrollOffset = 0;
                }
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.Backspace && _filterText.Length > 0)
            {
                _filterText = _filterText[..^1];
                UpdateFilter();
                if (_filteredIndices.Count > 0)
                {
                    _filteredIndex = 0;
                    _scrollOffset = 0;
                }
                MarkDirty();
                return;
            }
        }

        private void CloseDropdown()
        {
            _isOpen = false;
            _filterText = "";
            _filteredIndex = -1;
            _filteredIndices.Clear();
            _scrollOffset = 0;
            MarkDirty();
            Window?.MarkFullRedraw();
        }

        public override void Activate()
        {
            if (!Enabled) return;
            if (_isOpen)
            {
                if (_filteredIndex >= 0 && _filteredIndex < _filteredIndices.Count)
                {
                    SelectedIndex = _filteredIndices[_filteredIndex];
                    SelectionChanged?.Invoke(SelectedIndex);
                }
                CloseDropdown();
            }
            else
            {
                _isOpen = true;
                _filterText = "";
                UpdateFilter();
                _scrollOffset = 0;
                MarkDirty();
            }
        }

        public override void Cancel()
        {
            if (_isOpen)
            {
                _isOpen = false;
                _filterText = "";
                _filteredIndex = -1;
                _filteredIndices.Clear();
                _scrollOffset = 0;
                MarkDirty();
                Window?.MarkFullRedraw();
            }
        }
    }
}