using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.Monogame.CustomSerializers
{
    class SpriteValueProvider : CustomValueProvider<Sprite>
    {
        public override Sprite? CreateObject(object v, WinterForgeVM executor)
        {
            string value = (string)v;
            if (value.Contains('/'))
            {
                // Format 1: "4278190335/10/10"
                string[] parts = value.Split('/');
                if (parts.Length != 3)
                    return null;

                uint packedColor = uint.Parse(parts[0]);
                int width = int.Parse(parts[1]);
                int height = int.Parse(parts[2]);

                uint[] pixels = Enumerable.Repeat(packedColor, width * height).ToArray();

                return MonoUtils.CreateTexture(width, height, pixels);
            }
            else if (value.Contains('_') && value.Contains('+'))
            {
                // Format 2: "0_0_0_0_...+width_height"
                string[] split = value.Split('+');
                if (split.Length != 2)
                    return null;

                string[] colorParts = split[0].Split('_');
                string[] sizeParts = split[1].Split('_');
                if (sizeParts.Length != 2)
                    return null;

                int width = int.Parse(sizeParts[0]);
                int height = int.Parse(sizeParts[1]);

                if (colorParts.Length != width * height)
                    return null;

                uint[] pixels = new uint[colorParts.Length];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = uint.Parse(colorParts[i]);

                return MonoUtils.CreateTexture(width, height, pixels);
            }

            return null;
        }
        public override object CreateString(Sprite sprite, ObjectSerializer serializer)
        {
            if (sprite!.IsExternalTexture)
                return $"{sprite.TexturePath}"!;

            GeneratedTextureData data = sprite.GeneratedTextureData;
            if(data is null && sprite.BackingTexture is not null)
                data = new GeneratedTextureData(sprite.BackingTexture);
            if (data.Pixels.All(x => x == data.Pixels[0]))
                return $"\"{data.Pixels[0]}/{data.Width}/{data.Height}\"";

            StringBuilder sb = new();
            bool first = true;
            foreach(var pix in data.Pixels)
            {
                if (!first)
                    sb.Append('_');
                first = false;
                sb.Append(pix);
            }
            sb.Append($"+{data.Width}_{data.Height};");
            return $"{sb}";
        }
    }
}
