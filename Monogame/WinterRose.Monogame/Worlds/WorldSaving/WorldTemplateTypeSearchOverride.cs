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
    public bool HasCustomParser => WorldTemplateObjectParsers.ParserExists(Type);

    internal WorldTemplateTypeSearchOverride(Type type, string identifier)
    {
        Type = type;
        Identifier = identifier;
    }

    internal string? GetParsedString(object obj)
    {
        string? parsed = WorldTemplateObjectParsers.GetParsed(Type, obj, Identifier);
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
