using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.Utility;

public static class HtmlImageLoader
{
    private static readonly HttpClient HTTP = new HttpClient();

    static Log log = new Log("HTML image loader");

    public static async Task<Sprite?> LoadSpriteFromUrlAsync(string url)
    {
        try
        {
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return null;

            await log.DebugAsync($"Fetching {url}");
            var bytes = await HTTP.GetByteArrayAsync(url);


            await log.DebugAsync($"Saving image file...");
            var ext = Path.GetExtension(url);
            if (string.IsNullOrEmpty(ext)) ext = ".img";
            var tmp = Path.Combine(Path.GetTempPath(), "winter_image_" + Guid.NewGuid().ToString("N") + ext);
            await File.WriteAllBytesAsync(tmp, bytes);


            await log.DebugAsync($"Loading image from disk");
            var sprite = await ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn("Main", () => SpriteCache.Get(tmp), ForgeThread.JobPriority.Normal, false);

            await log.DebugAsync(sprite.Texture.Id == 0 ? "\\c[red]Failure" : "\\c[00FF00]Success");
            return sprite;
        }
        catch
        {
            return null;
        }
    }
}
