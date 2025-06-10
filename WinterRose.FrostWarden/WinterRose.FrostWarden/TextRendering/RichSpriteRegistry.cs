namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;

public static class RichSpriteRegistry
{
    private static Dictionary<string, Texture2D> spriteMap = new();

    public static void RegisterSprite(string key, Texture2D texture)
    {
        spriteMap[key] = texture;
    }

    public static Texture2D? GetSprite(string key)
    {
        return spriteMap.TryGetValue(key, out var tex) ? tex : null;
    }
}
