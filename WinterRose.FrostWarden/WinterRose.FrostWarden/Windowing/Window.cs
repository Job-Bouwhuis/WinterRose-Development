using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.Windowing
{
    public class Window
    {
        public int Width => ray.GetScreenWidth();
        public int Height => ray.GetScreenHeight();
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

        public Window(string title, ConfigFlags configFlags = 0)
        {
            Title = title;
            this.configFlags = configFlags;
        }

        public void Create(int width, int height)
        {
            Raylib.SetConfigFlags(configFlags);
            Raylib.InitWindow(width, height, Title);
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
            var pos = ray.GetWindowPosition();

            int width = Width, height = Height;

            Close();
            configFlags = newFlags;
            Create(width, height);

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
