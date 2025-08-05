using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.AssetPipeline;

namespace WinterRose.FrostWarden
{
    public static class SpriteCache
    {
        private static readonly Dictionary<string, Sprite> cache = [];
        private static readonly Dictionary<string, Texture2D> rawTextureCache = [];

        public static Sprite Get(string source)
        {
            if (cache.TryGetValue(source, out var sprite))
                return sprite;

            if(rawTextureCache.TryGetValue(source, out Texture2D tex))
                return new Sprite(tex, false);

            // Determine whether it's a generated or file-based sprite
            Sprite newSprite;

            if (source.StartsWith("Generated_"))
                newSprite = CreateGeneratedSpriteFromKey(source);
            else if (Assets.Exists(source))
                newSprite = Assets.Load<Sprite>(source);
            else
                newSprite = new Sprite(ray.LoadTexture(source), false);

            cache[source] = newSprite;
            return newSprite;
        }

        public static Sprite GetGenerated(int width, int height, Color fillColor)
        {
            string key = $"Generated_{width}_{height}_{fillColor.R}{fillColor.G}{fillColor.B}{fillColor.A}";
            return Get(key); // will go through CreateGeneratedSpriteFromKey internally
        }

        public static void RegisterSprite(Sprite sprite) => cache[sprite.Source] = sprite;

        private static Sprite CreateGeneratedSpriteFromKey(string key)
        {
            // Format: "Generated_width_height_RGBA"
            var parts = key.Split('_');
            if (parts.Length != 3 || !parts[0].Equals("Generated", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid generated sprite key: {key}");

            var size = parts[1].Split('x'); // support format like "64x32" if needed
            int width = int.Parse(size[0]);
            int height = int.Parse(size[1]);

            string rgba = parts[2];
            byte r = byte.Parse(rgba[..1]);
            byte g = byte.Parse(rgba[1..1]);
            byte b = byte.Parse(rgba[2..1]);
            byte a = byte.Parse(rgba[3..1]);
            Color fillColor = new Color(r, g, b, a);

            return Sprite.CreateRectangle(width, height, fillColor);
        }

        public static void DisposeAll()
        {
            foreach (var sprite in cache.Values)
            {
                if(!sprite.OwnsTexture && sprite is not SpriteGif)
                    ray.UnloadTexture(sprite.Texture);
                sprite.Dispose();
            }
            cache.Clear();

            foreach(var tex in rawTextureCache.Values)
                ray.UnloadTexture(tex);
            rawTextureCache.Clear();
        }

        public static void RegisterTexture2D(string key, Texture2D texture) => rawTextureCache.Add(key, texture);
    }

}
