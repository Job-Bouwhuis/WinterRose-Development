using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Data;

namespace WinterRose.ImGuiApps
{
    /// <summary>
    /// Renders a gif file by seperating its frames into seperate images and rendering them one by one.
    /// </summary>
    public class GifRenderer
    {
        private unsafe struct FrameData
        {
            public int width;
            public int height;
            public nint handle;
            public string path;
        }

        private List<FrameData> frames = new();
        int currentFrame = 0;
        float frameTime = 0;
        float NextFrameTime = 0.016f;
        Task? GifLoadTask = null;
        int framesLoaded = 0;
        int totalFrames = 0;
        public string originalGifPath;

        private static Dictionary<string, GifRenderer> renderers = [];


        internal static void Dispose()
        {
            foreach (var renderer in renderers.Values)
            {
                foreach (var frame in renderer.frames)
                {
                    Application.Current.RemoveImage(frame.path);
                    if (File.Exists(frame.path))
                        File.Delete(frame.path);

                }

                DirectoryInfo frameFolder = new(Path.Combine("Cache", Path.GetFileNameWithoutExtension(renderer.originalGifPath) + "_FRAMES"));
                if (frameFolder.Exists)
                    frameFolder.Delete();
            }
        }

        private async Task LoadGif(string path)
        {
            await Task.Run(() =>
            {
                if (path == null)
                    return;

                if (!File.Exists(path))
                    return;
                Image<Rgba32> gif;
TryAgain:
                try
                {
                    gif = Image.Load<Rgba32>(path);
                }
                catch (IOException)
                {
                    goto TryAgain;
                }

                FrameData[] frames = new FrameData[gif.Frames.Count];

                DirectoryInfo frameFolder = new(Path.Combine("Cache", Path.GetFileNameWithoutExtension(path) + "_FRAMES"));
                if (!frameFolder.Exists)
                    frameFolder.Create();
                frameFolder.Attributes |= FileAttributes.Hidden;

                int frameIndex = 0;
                totalFrames = gif.Frames.Count;
                foreach (var frame in gif.Frames)
                {
                    using Image<Rgba32> frameImage = new(gif.Width, gif.Height);
                    frame.Width.Repeat(x => frame.Height.Repeat(y => frameImage[x, y] = frame[x, y]));

                    string framePath = Path.Combine(frameFolder.FullName, $"{frameIndex}.png");
                    frameImage.Save(framePath, new PngEncoder());

                    Application.Current.AddOrGetImagePointer(framePath, false, out nint handle, out uint width, out uint height);

                    frames[frameIndex] = new FrameData
                    {
                        width = (int)width,
                        height = (int)height,
                        path = framePath,
                        handle = handle
                    };
                    originalGifPath = path;
                    frameIndex += 1;

                    framesLoaded += 1;
                }

                this.frames = [.. frames];
                gif.Dispose();
            });
        }

        /// <summary>
        /// Draws a gif file
        /// </summary>
        /// <param name="path">The path to the file where to find the gif</param>
        /// <param name="width">A width override. if 0, draws the original width of the gif</param>
        /// <param name="height">A height override. if 0, draws the original height of the gif</param>
        /// <param name="fps">The FPS the gif will attempt to run at. Leave 0 for default of 60</param>
        public static void Gif(string path, int width = 0, int height = 0, int fps = 0)
        {
            if (renderers.TryGetValue(path, out var existingRenderer))
            {
                existingRenderer.GifLoadTask ??= existingRenderer.LoadGif(path);
                existingRenderer.Draw(width, height);
                existingRenderer.NextFrameTime = 1f / fps;
                return;
            }

            GifRenderer renderer = new();
            renderers[path] = renderer;
            renderer.GifLoadTask = renderer.LoadGif(path);
            renderer.Draw(width, height);

            if (fps != 0)
                renderer.NextFrameTime = 1f / fps;
        }

        public static (int width, int height) GetSize(string gif)
        {
            if(renderers.TryGetValue(gif, out var renderer))
            {
                if (renderer.frames.Count == 0)
                    return (0, 0);

                return (renderer.frames[0].width, renderer.frames[0].height);
            }
            return (0, 0);
        }

        private void Draw(int width, int height)
        {
            if (GifLoadTask is { IsCompleted: false })
            {
                string text = $"Loading frames... ({framesLoaded}/{totalFrames}";
                int textWidth = gui.CalcTextSize(text).X.FloorToInt();
                gui.TextColored(Color.Yellow, text);
                // calc
                gui.ProgressBar(framesLoaded / (float)totalFrames, new System.Numerics.Vector2(textWidth, 15));
                return;
            }

            if (frames.Count == 0)
                return;

            if (frameTime >= NextFrameTime)
            {
                currentFrame += 1;
                if (currentFrame >= frames.Count)
                    currentFrame = 0;

                frameTime = 0;
            }

            FrameData frame = frames[currentFrame];

            width = width == 0 ? frame.width : width;
            height = height == 0 ? frame.height : height;

            gui.Image(frame.handle, new System.Numerics.Vector2(width, height));

            frameTime += Time.DeltaTime;
        }
    }
}
