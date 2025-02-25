using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Animations;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.Worlds
{
    public static class WorldTemplateObjectParsers
    {
        private static Dictionary<Type, Func<object, string, string>> parsers = [];

        static WorldTemplateObjectParsers()
        {
            DefaultParsers();
        }

        public static void Add(Type type, Func<object, string, string> parser) => parsers[type] = parser;

        public static bool ParserExists(Type t)
        {
            bool exists = parsers.ContainsKey(t);
            if (exists) return exists;
            else return t.Name.StartsWith("StaticCombinedModifier")
                || t.Name.StartsWith("StaticAdditiveModifier")
                || t.Name.StartsWith("StaticSubtractiveModifier")
                || t.Name.StartsWith("StaticMultiplicativeModifier");
        }

        public static string? GetParsed(Type t, object instance, string identifier)
        {
            if(parsers.TryGetValue(t, out var result))
                return result(instance, identifier);

            if (t.Name.StartsWith("StaticCombinedModifier")
                || t.Name.StartsWith("StaticAdditiveModifier")
                || t.Name.StartsWith("StaticSubtractiveModifier")
                || t.Name.StartsWith("StaticMultiplicativeModifier"))
            {
                return GetParsed(typeof(StaticCombinedModifier<>), instance, identifier);
            }
                return null;
        }

        private static void DefaultParsers()
        {
            Add(typeof(Sprite), (obj, identifier) =>
            {
                Sprite sprite = obj as Sprite;
                if (sprite.IsExternalTexture)
                    return $"{identifier}(\"{sprite.TexturePath}\")";

                var colors = sprite.GetPixelData();
                if (equal(colors))
                {
                    Color c = colors[0];
                    return $"{identifier}({sprite.Width}, {sprite.Height}, \"{Convert.ToHexString(new byte[4] { c.R, c.G, c.B, c.A })}\")";
                }
                List<byte> bytes = new();
                foreach (var color in colors)
                {
                    bytes.AddMany(color.R, color.G, color.B, color.A);
                }
                return $"{identifier}(\"{sprite.Width}^{sprite.Height}^{Convert.ToBase64String(bytes.ToArray()).Replace('=', '♥')}\")";

                bool equal(Color[] colors)
                {
                    Color first = colors[0];
                    return colors.All(x =>
                    {
                        return first.R == x.R && first.G == x.G && first.B == x.B && first.A == x.A;
                    });
                }
            });

            Add(typeof(Vector2), (obj, identifier) =>
            {
                Vector2 vec = (Vector2)obj;
                return $"{identifier}({MathF.Round(vec.X, 3).ToString().Replace(',', '.')}f, {MathF.Round(vec.Y, 3).ToString().Replace(',', '.')}f)";
            });

            Add(typeof(Vector2I), (obj, identifier) =>
            {
                Vector2I vec = (Vector2I)obj;
                return $"{identifier}({vec.X}, {vec.Y})";
            });
            Add(typeof(Point), (obj, identifier) =>
            {
                Point point = (Point)obj;
                return $"{identifier}({point.X}, {point.Y})";
            });

            Add(typeof(Color), (obj, identifier) =>
            {
                Color col = (Color)obj;
                return $"{identifier}({col.PackedValue})";
            });

            Add(typeof(SpriteSheet), (obj, identifier) =>
            {
                SpriteSheet sheet = (SpriteSheet)obj;
                return $"{identifier}(\"{sheet.SourceSprite.TexturePath}\", {sheet.Width}, {sheet.Height}, {sheet.EdgeMargin}, {sheet.PaddingBetweenSprites})";
            });

            Add(typeof(float), (obj, identifier) =>
            {
                return $"{MathF.Round((float)obj, 3).ToString().Replace('.', ',')}f";
            });

            Add(typeof(double), (obj, identifier) =>
            {
                return $"{Math.Round((double)obj, 3).ToString().Replace('.', ',')}d";
            });

            Add(typeof(long), (obj, identifier) =>
            {
                return $"{(long)obj}L";
            });

            Add(typeof(Transform), (obj, identifier) =>
            {
                return $"{((Transform)obj).owner.Name}.transform";
            });

            Add(typeof(Rectangle), (obj, identifier) =>
            {
                Rectangle rect = (Rectangle)obj;
                return $"{identifier}({rect.X}, {rect.Y}, {rect.Width}, {rect.Height})";
            });

            Add(typeof(AnimationController), (obj, identifier) =>
            {
                AnimationController controller = (AnimationController)obj;
                return $"{identifier}(\"{string.Join("\", \"", controller.Animations.Select(x => x.Name))}\")";
            });

            AddModifierType(typeof(StaticCombinedModifier<>));
            AddModifierType(typeof(StaticAdditiveModifier<>));
            AddModifierType(typeof(StaticSubtractiveModifier<>));
            AddModifierType(typeof(StaticMultiplicativeModifier<>));

            void AddModifierType(Type type)
            {
                Add(type, (obj, identifier) =>
                {
                    object val = ((dynamic)obj).Value;
                    StringBuilder result = new(val.ToString().Replace('.', ','));
                    result.Append(val switch
                    {
                        float => "f",
                        double => "d",
                        _ => ""
                    });
                    return result.ToString();
                });
            }
        }
    }
}
