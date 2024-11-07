using Microsoft.Xna.Framework;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Animations;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A type override for the <see cref="WorldTemplate"/> system.
/// </summary>
[DebuggerDisplay("{Identifier} = {Type.FullName}")]
public class WorldTemplateTypeSearchOverride
{
    /// <summary>
    /// The type to specify the override for.
    /// </summary>
    public Type Type { get; private set; }
    /// <summary>
    /// The identifier to use for the type.
    /// </summary>
    internal string Identifier { get; private set; }
    /// <summary>
    /// Whether or not this type has a custom parser. e.g. <see cref="Vector2"/> has a custom parser. making it parsed as "Vector2(0, 0)" instead of "{0, 0}".
    /// </summary>
    public bool HasCustomParser => ParseToString is not null;

    /// <summary>
    /// The custom parser for this type. e.g. <see cref="Vector2"/> has a custom parser. making it parsed as "Vector2(0, 0)" instead of "{0, 0}".
    /// </summary>
    public Func<object, string, string> ParseToString;

    internal WorldTemplateTypeSearchOverride(Type type, string identifier)
    {
        Type = type;
        Identifier = identifier;
        defaultParsers();
    }

    internal void defaultParsers()
    {
        if (Type == typeof(Vector2))
        {
            ParseToString = (obj, identifier) =>
            {
                Vector2 vec = (Vector2)obj;
                return $"{identifier}({MathF.Round(vec.X, 3).ToString().Replace(',', '.')}f, {MathF.Round(vec.Y, 3).ToString().Replace(',', '.')}f)";
            };
        }
        if (Type == typeof(Vector2I))
        {
            ParseToString = (obj, identifier) =>
            {
                Vector2I vec = (Vector2I)obj;
                return $"{identifier}({vec.X}, {vec.Y})";
            };
        }
        if (Type == typeof(Point))
        {
            ParseToString = (obj, identifier) =>
            {
                Point point = (Point)obj;
                return $"{identifier}({point.X}, {point.X})";
            };
        }
        if (Type == typeof(Color))
        {
            ParseToString = (obj, identifier) =>
            {
                Color col = (Color)obj;

                return $"{identifier}({col.PackedValue})";
            };
        }
        if (Type == typeof(Sprite))
        {
            ParseToString = (obj, identifier) =>
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
            };
        }
        if (Type == typeof(SpriteSheet))
        {
            ParseToString = (obj, identifier) =>
            {
                SpriteSheet sheet = obj as SpriteSheet;
                return $"{identifier}(\"{sheet.SourceSprite.TexturePath}\", {sheet.Width}, {sheet.Height}, {sheet.EdgeMargin}, {sheet.PaddingBetweenSprites})";
            };
        }
        if (Type == typeof(float))
        {
            ParseToString = (obj, identifier) => $"{MathF.Round((float)obj, 3).ToString().Replace('.', ',')}f";
        }
        if (Type == typeof(double))
        {
            ParseToString = (obj, identifier) => $"{Math.Round((double)obj, 3).ToString().Replace('.', ',')}d";
        }
        if (Type == typeof(long))
        {
            ParseToString = (obj, identifier) => $"{(long)obj}L";
        }
        if (Type == typeof(Transform))
        {
            ParseToString = (obj, identifier) => $"{((Transform)obj).owner.Name}.transform";
        }
        if (Type == typeof(Rectangle))
        {
            ParseToString = (obj, identifier) =>
            {
                Rectangle rect = (Rectangle)obj;
                return $"{identifier}({rect.X}, {rect.Y}, {rect.Width}, {rect.Height})";
            };
        }
        if (Type == typeof(AnimationController))
        {
            ParseToString = (obj, identifier) =>
            {
                AnimationController controller = (AnimationController)obj;
                return $"{identifier}(\"{string.Join("\", \"", controller.Animations.Select(x => x.Name))}\")";
            };
        }
        if (Type.Name.StartsWith("StaticCombinedModifier")
            || Type.Name.StartsWith("StaticAdditiveModifier")
            || Type.Name.StartsWith("StaticSubtractiveModifier")
            || Type.Name.StartsWith("StaticMultiplicativeModifier"))
        {
            ParseToString = (obj, identifier) =>
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
            };
        }
    }
    public WorldTemplateTypeSearchOverride(Type type, string identifyer, Func<object, string, string> parseToString) : this(type, identifyer)
    {
        ParseToString = parseToString;
    }
    internal string? GetParsedString(object obj)
    {
        string? parsed = ParseToString?.Invoke(obj, Identifier);
        if (parsed is null) return obj.ToString();
        return parsed;
    }
    internal static List<WorldTemplateTypeSearchOverride> GetDefinitions(string[] data)
    {
        List<WorldTemplateTypeSearchOverride> defs = new List<WorldTemplateTypeSearchOverride>();

        foreach (string over in data)
        {
            string[] def = over.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Type t = TypeWorker.FindType(def[1]);
            defs.Add(new(t, def[0]));
        }
        return defs;
    }
    internal static async Task<List<WorldTemplateTypeSearchOverride>> GetDefinitionsAsync(string[] data)
    {
        List<WorldTemplateTypeSearchOverride> defs = new List<WorldTemplateTypeSearchOverride>();

        List<Task<List<WorldTemplateTypeSearchOverride>>> tasks = new();

        foreach (string over in data)
        {
            string[] def = over.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Type t = TypeWorker.FindType(def[1]);
            defs.Add(new(t, def[0]));
        }
        return defs;
    }
}
