using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using EnvDTE;

namespace WinterRose.ForgeWarden.Utility;

public static class HttpImageLoader
{
    private static readonly HttpClient HTTP;
    static HttpImageLoader()
    {
        HTTP = new HttpClient();
        HTTP.Timeout = TimeSpan.FromSeconds(10);
        HTTP.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
    }

    public static async Task<Sprite?> LoadSpriteFromUrlAsync(string url)
    {
        string tmp = Path.Combine(Path.GetTempPath(), "WinterRose_Images", "winter_image_" + Guid.NewGuid().ToString("N") + ".png");
        if(!Directory.Exists(Path.GetDirectoryName(tmp)))
            Directory.CreateDirectory(Path.GetDirectoryName(tmp));

        byte[] bytes = null;
        try
        {
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return null;

            // download raw bytes
            bytes = await HTTP.GetByteArrayAsync(url);

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
                return s;
            }, ForgeThread.JobPriority.Normal, false);

            return sprite;
        }
        catch (Exception ex)
        {
            if(tmp is not null)
            {
                try
                {
                    ConvertIcoToPng(bytes, tmp);
                }
                catch { }


                // try raylib itself
                var sprite = await ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn("Main", () =>
                {
                    Sprite s = SpriteCache.Get(tmp, true);
                    return s;
                }, ForgeThread.JobPriority.Normal, false);

                if (sprite.Texture.Id != 0)
                    return sprite;
            }
            new Log("Http ImageLoader").Error(ex);
        
            return null;
        }
    }

    public static void ConvertIcoToPng(byte[] icoBytes, string pngPath)
    {
        using var stream = new MemoryStream(icoBytes, writable: false);
        using var reader = new BinaryReader(stream);

        reader.ReadUInt16(); // reserved
        ushort type = reader.ReadUInt16();
        ushort count = reader.ReadUInt16();

        if (type != 1 || count == 0)
            throw new InvalidDataException("Not a valid ICO file");

        int bestSize = -1;
        long bestOffset = 0;
        int bestLength = 0;

        for (int i = 0; i < count; i++)
        {
            byte width = reader.ReadByte();
            byte height = reader.ReadByte();
            reader.ReadByte(); // color count
            reader.ReadByte(); // reserved
            reader.ReadUInt16(); // planes
            reader.ReadUInt16(); // bit count
            int bytesInRes = reader.ReadInt32();
            int imageOffset = reader.ReadInt32();

            int actualWidth = width == 0 ? 256 : width;
            int actualHeight = height == 0 ? 256 : height;
            int sizeScore = actualWidth * actualHeight;

            if (sizeScore > bestSize)
            {
                bestSize = sizeScore;
                bestOffset = imageOffset;
                bestLength = bytesInRes;
            }
        }

        stream.Position = bestOffset;
        byte[] imageData = reader.ReadBytes(bestLength);

        using var image = SixLabors.ImageSharp.Image.Load(imageData);
        image.SaveAsPng(pngPath);
    }

}
