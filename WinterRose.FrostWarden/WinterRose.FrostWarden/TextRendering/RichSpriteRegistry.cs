namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using WinterRose.ForgeGuardChecks;

public static class RichSpriteRegistry
{
    private static Dictionary<string, Sprite> spriteMap = new();

    public static void RegisterSprite(string key, Sprite texture)
    {
        spriteMap[key] = texture;
    }

    /// <summary>
    /// Gets a sprite reference for rich text rendering. if the given key wasnt registered to a sprite, throws the exception
    /// </summary>
    /// <param name="key">The key identifying the sprite to retrieve.</param>
    /// <exception cref="ForgeGuardChecks.Exceptions.ValueNullException"></exception>
    /// <returns>The sprite associated with the given key.</returns>
    public static Sprite GetSprite(string key)
    {
        bool found = spriteMap.TryGetValue(key, out var sprite);
        Forge.Expect(found && sprite != null).True();
        return sprite!;
    }

    /// <summary>
    /// Gets the source for the given <paramref name="spriteKey"/>
    /// </summary>
    /// <param name="spriteKey">The key used to look up the sprite.</param>
    /// /// <exception cref="ForgeGuardChecks.Exceptions.ValueNullException"></exception>
    /// <returns>The file path reference for the sprite. or "Generated_width_height_RGBA" if generated</returns>
    internal static string GetSourceFor(string spriteKey) => GetSprite(spriteKey).Source;
}
