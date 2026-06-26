namespace RandomTesting.ConsoleWindowing
{
    // ========== SCREEN MANAGER ==========
    public static class Screen
    {
        public static List<Window> windows = new List<Window>();
        private static int _focusedWindowIndex = -1;
        private static Control _focusedControl = null;
        private static int _lastConsoleWidth;
        private static int _lastConsoleHeight;
        private static bool _cursorVisible = true;
        private static DateTime _lastBlinkTime = DateTime.Now;
        private static bool _needFullClear = false;

        // Window selection mode
        private static bool _windowSelectMode = false;
        private static int _selectedWindowIndex = -1;
        private static bool _windowBorderBlink = true;

        // Shutdown modal
        private static Window _shutdownWindow = null;
        private static int _shutdownCountdown = 5;
        private static DateTime _shutdownLastTick = DateTime.Now;
        private static bool _shutdownCancelled = false;
        private static bool _shutdownConfirmed = false;
        private static bool _isShuttingDown = false;
        private static Label _shutdownCountdownLabel = null;

        public static void AddWindow(Window window)
        {
            window.ZIndex = windows.Count > 0 ? windows.Max(w => w.ZIndex) + 1 : 1;
            window.AdjustSize(Console.WindowWidth, Console.WindowHeight);
            windows.Add(window);
            if (_focusedWindowIndex < 0) FocusWindow(windows.Count - 1);
            window.MarkFullRedraw();
        }

        public static void FocusWindow(int index)
        {
            if (index < 0 || index >= windows.Count) return;
            var win = windows[index];
            int maxZ = windows.Max(w => w.ZIndex);
            win.ZIndex = maxZ + 1;
            windows = windows.OrderByDescending(w => w.ZIndex).ToList();
            _focusedWindowIndex = windows.IndexOf(win);
            win.OnFocus();
            var first = win.GetAllFocusableControls().FirstOrDefault();
            if (first != null) SetFocus(first);
            else SetFocus(null);
            win.MarkDirty();
        }

        public static void RemoveWindow(Window window)
        {
            windows.Remove(window);
            _needFullClear = true;
            if (windows.Count == 0) return;
            var top = windows.OrderByDescending(w => w.ZIndex).First();
            FocusWindow(windows.IndexOf(top));
            foreach (var w in windows) w.MarkFullRedraw();
        }

        private static void SetFocus(Control newFocus)
        {
            if (_focusedControl != null)
            {
                _focusedControl.IsFocused = false;
                _focusedControl.OnBlur();
                _focusedControl.MarkDirty();
            }
            _focusedControl = newFocus;
            if (newFocus != null)
            {
                newFocus.IsFocused = true;
                newFocus.OnFocus();
                newFocus.MarkDirty();
                _lastBlinkTime = DateTime.Now;
                _cursorVisible = true;
            }
        }

        public static void ProcessInput()
        {
            if (_shutdownWindow != null)
            {
                if (!Console.KeyAvailable) return;
                var k = Console.ReadKey(true);
                _shutdownWindow.HandleShutdownKey(k);
                return;
            }

            if (!Console.KeyAvailable) return;
            var key = Console.ReadKey(true);

            // Window select mode
            if (_windowSelectMode)
            {
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A when key.Modifiers == 0:
                        MoveWindowSelection(Direction.Left);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D when key.Modifiers == 0:
                        MoveWindowSelection(Direction.Right);
                        break;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W when key.Modifiers == 0:
                        MoveWindowSelection(Direction.Up);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S when key.Modifiers == 0:
                        MoveWindowSelection(Direction.Down);
                        break;
                    case ConsoleKey.Enter:
                        if (_selectedWindowIndex >= 0)
                        {
                            _windowSelectMode = false;
                            FocusWindow(_selectedWindowIndex);
                            foreach (var w in windows) w.MarkDirty();
                        }
                        break;
                    case ConsoleKey.Escape:
                        _windowSelectMode = false;
                        foreach (var w in windows) w.MarkDirty();
                        break;
                }
                return;
            }

            // Tab handling
            if (key.Key == ConsoleKey.Tab)
            {
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    _windowSelectMode = true;
                    _selectedWindowIndex = _focusedWindowIndex;
                    _windowBorderBlink = true;
                    _lastBlinkTime = DateTime.Now;
                    foreach (var w in windows) w.MarkDirty();
                    return;
                }
                else
                {
                    CycleFocus(1);
                    return;
                }
            }

            // If focused control consumes navigation, let it handle
            if (_focusedControl is IConsumesNavigation navControl && navControl.ConsumesNavigation)
            {
                _focusedControl.HandleKey(key);
                _focusedControl.MarkDirty();
                return;
            }

            // Global navigation and scrolling
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A when key.Modifiers == 0:
                    MoveFocus(Direction.Left);
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D when key.Modifiers == 0:
                    MoveFocus(Direction.Right);
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.W when key.Modifiers == 0:
                    MoveFocus(Direction.Up);
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S when key.Modifiers == 0:
                    MoveFocus(Direction.Down);
                    break;
                case ConsoleKey.PageUp:
                    if (_focusedWindowIndex >= 0)
                        windows[_focusedWindowIndex].ScrollPageUp();
                    break;
                case ConsoleKey.PageDown:
                    if (_focusedWindowIndex >= 0)
                        windows[_focusedWindowIndex].ScrollPageDown();
                    break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    _focusedControl?.Activate();
                    _focusedControl?.MarkDirty();
                    break;
                case ConsoleKey.Escape:
                    if (_focusedControl != null)
                    {
                        _focusedControl.Cancel();
                        _focusedControl.MarkDirty();
                    }
                    break;
                default:
                    _focusedControl?.HandleKey(key);
                    _focusedControl?.MarkDirty();
                    break;
            }
        }

        private enum Direction { Left, Right, Up, Down }

        private static void MoveFocus(Direction dir)
        {
            if (_focusedWindowIndex < 0 || _focusedWindowIndex >= windows.Count) return;
            var window = windows[_focusedWindowIndex];
            var focusables = window.GetAllFocusableControls().ToList();
            if (focusables.Count == 0) return;

            if (_focusedControl == null || !focusables.Contains(_focusedControl))
            {
                var first = focusables.First();
                window.ScrollToControl(first);
                SetFocus(first);
                return;
            }

            var current = _focusedControl;
            var (currentX, currentY) = current.GetAbsolutePosition();
            Control best = null;
            double bestScore = double.MaxValue;

            foreach (var c in focusables)
            {
                if (c == current) continue;
                var (cX, cY) = c.GetAbsolutePosition();
                double dx = cX - currentX;
                double dy = cY - currentY;

                bool isCandidate = dir switch
                {
                    Direction.Left => dx < 0,
                    Direction.Right => dx > 0,
                    Direction.Up => dy < 0,
                    Direction.Down => dy > 0,
                    _ => false
                };
                if (!isCandidate) continue;

                double primary = dir switch
                {
                    Direction.Left or Direction.Right => Math.Abs(dx),
                    Direction.Up or Direction.Down => Math.Abs(dy),
                    _ => 0
                };
                double secondary = dir switch
                {
                    Direction.Left or Direction.Right => Math.Abs(dy),
                    Direction.Up or Direction.Down => Math.Abs(dx),
                    _ => 0
                };

                double score = secondary * 10 + primary;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            if (best != null)
            {
                window.ScrollToControl(best);
                SetFocus(best);
            }
        }

        private static void CycleFocus(int direction)
        {
            if (_focusedWindowIndex < 0 || _focusedWindowIndex >= windows.Count) return;
            var window = windows[_focusedWindowIndex];
            var focusables = window.GetAllFocusableControls().ToList();
            if (focusables.Count == 0) return;
            int idx = focusables.IndexOf(_focusedControl);
            if (idx < 0) idx = direction > 0 ? -1 : focusables.Count;
            idx = (idx + direction + focusables.Count) % focusables.Count;
            var target = focusables[idx];
            window.ScrollToControl(target);
            SetFocus(target);
        }

        private static void MoveWindowSelection(Direction dir)
        {
            if (windows.Count == 0) return;
            int current = _selectedWindowIndex;
            if (current < 0) current = 0;

            double bestScore = double.MaxValue;
            int bestIdx = -1;
            for (int i = 0; i < windows.Count; i++)
            {
                if (i == current) continue;
                double dx = windows[i].X - windows[current].X;
                double dy = windows[i].Y - windows[current].Y;
                bool ok = dir switch
                {
                    Direction.Left => dx < 0 && Math.Abs(dy) <= Math.Abs(dx) * 2,
                    Direction.Right => dx > 0 && Math.Abs(dy) <= Math.Abs(dx) * 2,
                    Direction.Up => dy < 0 && Math.Abs(dx) <= Math.Abs(dy) * 2,
                    Direction.Down => dy > 0 && Math.Abs(dx) <= Math.Abs(dy) * 2,
                    _ => false
                };
                if (!ok) continue;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                double targetAngle = dir switch
                {
                    Direction.Left => 180,
                    Direction.Right => 0,
                    Direction.Up => -90,
                    Direction.Down => 90,
                    _ => 0
                };
                double diff = Math.Abs(angle - targetAngle);
                if (diff > 180) diff = 360 - diff;
                double score = diff * 3 + dist * 0.5;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIdx = i;
                }
            }

            if (bestIdx == -1)
            {
                double bestDist = double.MaxValue;
                foreach (int i in Enumerable.Range(0, windows.Count))
                {
                    if (i == current) continue;
                    double dx = windows[i].X - windows[current].X;
                    double dy = windows[i].Y - windows[current].Y;
                    bool ok = dir switch
                    {
                        Direction.Left => dx < 0,
                        Direction.Right => dx > 0,
                        Direction.Up => dy < 0,
                        Direction.Down => dy > 0,
                        _ => false
                    };
                    if (!ok) continue;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestIdx = i;
                    }
                }
                if (bestIdx == -1)
                    bestIdx = (current + 1) % windows.Count;
            }

            _selectedWindowIndex = bestIdx;
            foreach (var w in windows) w.MarkDirty();
        }

        public static void Update()
        {
            if (_needFullClear)
            {
                Console.Clear();
                _needFullClear = false;
            }

            int w = Console.WindowWidth;
            int h = Console.WindowHeight;
            if (w != _lastConsoleWidth || h != _lastConsoleHeight)
            {
                _lastConsoleWidth = w;
                _lastConsoleHeight = h;
                foreach (var win in windows)
                {
                    win.AdjustSize(w, h);
                    win.MarkFullRedraw();
                }
                if (_shutdownWindow != null)
                {
                    _shutdownWindow.AdjustSize(w, h);
                    _shutdownWindow.MarkFullRedraw();
                }
            }

            // Update shutdown countdown
            if (_shutdownWindow != null && !_shutdownCancelled && !_shutdownConfirmed)
            {
                if ((DateTime.Now - _shutdownLastTick).TotalSeconds >= 1)
                {
                    _shutdownCountdown--;
                    _shutdownLastTick = DateTime.Now;
                    if (_shutdownCountdown <= 0)
                    {
                        CleanupAndExit();
                        return;
                    }
                    if (_shutdownCountdownLabel != null)
                    {
                        _shutdownCountdownLabel.Text = $"Countdown: {_shutdownCountdown} seconds";
                        _shutdownCountdownLabel.MarkDirty();
                    }
                    _shutdownWindow.MarkDirty();
                }
            }

            // Blink cursor for TextInput / Dropdown
            if (_focusedControl is TextInput ti && ti.IsActive)
            {
                if ((DateTime.Now - _lastBlinkTime).TotalMilliseconds >= 500)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastBlinkTime = DateTime.Now;
                    ti.MarkDirty();
                }
            }
            else if (_focusedControl is Dropdown dd && dd.ConsumesNavigation)
            {
                if ((DateTime.Now - _lastBlinkTime).TotalMilliseconds >= 500)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastBlinkTime = DateTime.Now;
                    dd.MarkDirty();
                }
            }
            else
                _cursorVisible = true;

            // Window border blink in select mode
            if (_windowSelectMode)
            {
                if ((DateTime.Now - _lastBlinkTime).TotalMilliseconds >= 500)
                {
                    _windowBorderBlink = !_windowBorderBlink;
                    _lastBlinkTime = DateTime.Now;
                    foreach (var win in windows)
                        win.MarkDirty();
                }
            }

            // Draw all windows (sorted by ZIndex)
            foreach (var win in windows.OrderByDescending(w => w.ZIndex))
                if (win.IsDirty)
                    win.Draw();

            // Draw shutdown window on top if present
            if (_shutdownWindow != null && _shutdownWindow.IsDirty)
                _shutdownWindow.Draw();
        }

        public static void Run()
        {
            Console.CursorVisible = false;
            _lastConsoleWidth = Console.WindowWidth;
            _lastConsoleHeight = Console.WindowHeight;
            while (true)
            {
                Update();
                ProcessInput();
                System.Threading.Thread.Sleep(20);
            }
        }

        public static void Stop()
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            int width = 50;
            int height = 9;
            int x = (Console.WindowWidth - width) / 2;
            int y = (Console.WindowHeight - height) / 2;
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            var shutdownWin = new Window(" Shutdown ", x, y, width, height);
            shutdownWin.BorderColor = ConsoleColor.Red;
            shutdownWin.TitleColor = ConsoleColor.Yellow;
            shutdownWin.BackgroundColor = ConsoleColor.Black;
            shutdownWin.ZIndex = 9999;

            var msgLabel = new Label("Shutdown in progress...");
            var countdownLabel = new Label($"Countdown: {_shutdownCountdown} seconds");
            var instructionLabel = new Label("Press ENTER to confirm now, ESC to cancel");

            var root = new LayerStack(Orientation.Vertical);
            root.AddChild(msgLabel);
            root.AddChild(countdownLabel);
            root.AddChild(instructionLabel);
            shutdownWin.SetRoot(root);

            _shutdownWindow = shutdownWin;
            _shutdownCountdown = 5;
            _shutdownLastTick = DateTime.Now;
            _shutdownCancelled = false;
            _shutdownConfirmed = false;
            _shutdownCountdownLabel = countdownLabel;

            shutdownWin.MarkFullRedraw();
        }

        public static void HandleShutdownKey(Window win, ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter)
            {
                _shutdownConfirmed = true;
                CleanupAndExit();
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                _shutdownCancelled = true;
                _shutdownWindow = null;
                _shutdownCountdownLabel = null;
                _isShuttingDown = false;
                _needFullClear = true;
                foreach (var w in windows) w.MarkFullRedraw();
            }
        }

        private static void CleanupAndExit()
        {
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.WriteLine();
            Environment.Exit(0);
        }

        public static bool IsCursorVisible() => _cursorVisible;
        public static bool IsWindowSelectMode() => _windowSelectMode;
        public static bool IsWindowSelected(int index) => _windowSelectMode && _selectedWindowIndex == index;
        public static bool IsBorderBlinkVisible() => _windowBorderBlink;
    }
}
