using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.DialogBoxes;
using WinterRose.FrostWarden.Worlds;

namespace WinterRose.FrostWarden.Windowing
{
    public class Window
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Title
        {
            get
            {
                return title;
            }

            private set
            {
                title = value;
                ray.SetWindowTitle(Title);
            }
        }

        public bool IsReady => ray.WindowShouldClose() == false;
        public bool IsFullscreen => ray.IsWindowFullscreen();
        public Vector2 Size => new(Width, Height);

        private ConfigFlags configFlags;
        private string title;

        public Window(int width, int height, string title, ConfigFlags configFlags = 0)
        {
            Width = width;
            Height = height;
            Title = title;
            this.configFlags = configFlags;
        }

        public void Create()
        {
            Raylib.SetConfigFlags(configFlags);
            Raylib.InitWindow(Width, Height, Title);
        }

        public void Close()
        {
            Raylib.CloseWindow();
        }

        public void ToggleFullscreen()
        {
            ray.ToggleFullscreen();
        }

        public void SetSize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            ray.SetWindowSize(Width, Height);
        }

        public void Center()
        {
            var monitor = ray.GetCurrentMonitor();
            var monitorWidth = ray.GetMonitorWidth(monitor);
            var monitorHeight = ray.GetMonitorHeight(monitor);
            ray.SetWindowPosition((monitorWidth - Width) / 2, (monitorHeight - Height) / 2);
        }

        public void RequestRecreate(ConfigFlags newFlags)
        {
            // Capture state you want to restore later here, like window position
            var pos = ray.GetWindowPosition();

            Close();
            configFlags = newFlags;
            Create();

            // Optionally reposition and restore any GL state
            ray.SetWindowPosition(pos.X.FloorToInt(), pos.Y.FloorToInt());
        }

        internal bool ShouldClose()
        {
            if (ray.WindowShouldClose())
            {
                return true;
            }
            return false;
        }
    }
}
