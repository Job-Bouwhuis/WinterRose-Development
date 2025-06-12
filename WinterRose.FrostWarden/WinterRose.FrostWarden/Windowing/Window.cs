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
        public string Id { get; private set; }
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
        public Camera? Camera { get; }
        public RenderTexture2D RenderTarget { get; private set; }

        public bool IsReady => ray.WindowShouldClose() == false;
        public bool IsFullscreen => ray.IsWindowFullscreen();
        public Vector2 Size => new(Width, Height);

        private ConfigFlags configFlags;
        private string title;

        public Window(string id, int width, int height, string title, Camera? camera)
        {
            Id = id;
            Width = width;
            Height = height;
            Title = title;
            Camera = camera;
        }

        public void Create()
        {
            Raylib.InitWindow(Width, Height, Title);
            RenderTarget = Raylib.LoadRenderTexture(Width, Height);
        }

        public void BeginDraw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            Raylib.BeginTextureMode(RenderTarget);
            Raylib.ClearBackground(Color.DarkGray);

            if(Camera is not null)
                Raylib.BeginMode2D(Camera.Camera2D);
        }

        public void EndDraw()
        {
            Raylib.EndMode2D();
            Raylib.EndTextureMode();

            Raylib.DrawTexturePro(
                RenderTarget.Texture,
                new Rectangle(0, 0, RenderTarget.Texture.Width, -RenderTarget.Texture.Height),
                new Rectangle(0, 0, Width, Height),
                Vector2.Zero,
                0,
                Color.White);

            Raylib.EndDrawing();
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
    }
}
