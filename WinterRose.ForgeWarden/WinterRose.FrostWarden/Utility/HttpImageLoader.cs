using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WinterRose.ForgeWarden.Utility;

public static class HttpImageLoader
{
    private static readonly HttpClient HTTP = new HttpClient();

    public static async Task<Sprite?> LoadSpriteFromUrlAsync(string url)
    {
        try
        {
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return null;

            // download raw bytes
            var bytes = await HTTP.GetByteArrayAsync(url);

            // create a temporary PNG path
            var tmp = Path.Combine(Path.GetTempPath(), "winter_image_" + Guid.NewGuid().ToString("N") + ".png");

            // convert to PNG using ImageSharp
            using (var ms = new MemoryStream(bytes))
            using (var image = await Image.LoadAsync<Rgba32>(ms))
            {
                await image.SaveAsync(tmp, new PngEncoder());
            }

            // invoke main-thread loading for GL / Raylib
            var sprite = await ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn("Main", () =>
            {
                Sprite s = SpriteCache.Get(tmp, true);
                if (s.Texture.Id == 0)
                {
                    // failed. set a breakpoint here if needed
                }
                return s;
            }, ForgeThread.JobPriority.Normal, false);

            return sprite;
        }
        catch (Exception ex)
        {
            new Log("Http ImageLoader").Error(ex);
            return null;
        }
    }
}
