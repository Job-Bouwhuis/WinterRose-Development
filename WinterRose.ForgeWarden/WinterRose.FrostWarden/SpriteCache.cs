using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.AssetPipeline;

namespace WinterRose.ForgeWarden
{
    public static class SpriteCache
    {
        private static readonly Dictionary<string, Sprite> cache = [];
        private static readonly Dictionary<string, Texture2D> rawTextureCache = [];

        private static readonly HashSet<string> temporaryFiles = new();

        public static Sprite Get(string source, bool temporary = false)
        {
            if (cache.TryGetValue(source, out var sprite))
                return sprite;

            if (rawTextureCache.TryGetValue(source, out Texture2D tex))
                return new Sprite(tex, false);

            Sprite newSprite;

            if (source.StartsWith("Generated_"))
                newSprite = CreateGeneratedSpriteFromKey(source);
            else if (Assets.Exists(source))
                newSprite = Assets.Load<Sprite>(source);
            else
            {
                if (!File.Exists(source))
                    throw new InvalidOperationException("File doesn't exist: " + source);

                newSprite = new Sprite(ray.LoadTexture(source), false);
                newSprite.Source = source;

                if (temporary)
                    temporaryFiles.Add(source);
            }

            cache[source] = newSprite;
            return newSprite;
        }
        public static void RegisterSprite(Sprite sprite) => cache[sprite.Source] = sprite;

        public static Sprite GetGenerated(int width, int height, Color fillColor)
        {
            uint packedColor =
                ((uint)fillColor.R << 24) |
                ((uint)fillColor.G << 16) |
                ((uint)fillColor.B << 8) |
                fillColor.A;

            string key = $"Generated_{width}_{height}_{packedColor:X8}";
            return Get(key);
        }

        private static Sprite CreateGeneratedSpriteFromKey(string key)
        {
            // Expected format: "Generated_width_height_COLORHEX"
            var parts = key.Split('_');
            if (parts.Length != 4 || !parts[0].Equals("Generated", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid generated sprite key: {key}");

            int width = int.Parse(parts[1]);
            int height = int.Parse(parts[2]);

            uint packedColor = Convert.ToUInt32(parts[3], 16);

            byte r = (byte)((packedColor >> 24) & 0xFF);
            byte g = (byte)((packedColor >> 16) & 0xFF);
            byte b = (byte)((packedColor >> 8) & 0xFF);
            byte a = (byte)(packedColor & 0xFF);

            var fillColor = new Color(r, g, b, a);
            Image img = Raylib.GenImageColor(width, height, fillColor);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            var sprite = new Sprite(tex, false);
            sprite.Source = key;
            SpriteCache.RegisterSprite(sprite);
            return sprite;
        }

        /// <summary>
        /// Clears all currently loaded sprites from memory, and if they were created as temporary, their file will be deleted
        /// </summary>
        public static void DisposeAll()
        {
            foreach (var sprite in cache.Values)
                DisposeOf(sprite);

            cache.Clear();

            foreach (var tex in rawTextureCache.Values)
                ray.UnloadTexture(tex);

            rawTextureCache.Clear();
            temporaryFiles.Clear();

        }

        public static void DisposeOf(Sprite? sprite)
        {
            if (sprite == null)
                return;

            if (!sprite.OwnsTexture && sprite is not SpriteGif)
                ray.UnloadTexture(sprite.Texture);

            if (temporaryFiles.FirstOrDefault(tf => tf == sprite.Source) is string tf)
            {
                try
                {
                    if (File.Exists(tf))
                        File.Delete(tf);
                }
                catch { }
                temporaryFiles.Remove(tf);
            }

            sprite.Dispose();
            GC.Collect();
        }

        public static void RegisterTexture2D(string key, Texture2D texture) => rawTextureCache.Add(key, texture);
    }

}
