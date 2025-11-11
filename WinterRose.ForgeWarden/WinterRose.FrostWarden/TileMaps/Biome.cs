using Raylib_cs;

namespace WinterRose.ForgeWarden.TileMaps;

// basic placeholder biome type (empty on purpose, can be extended later)

public class RedTile(Sprite sprite) : Tile("RedTile", sprite);
public class YellowTile(Sprite sprite) : Tile("YellowTile", sprite);

public class Biome
{
    private readonly Sprite sprite;

    /// <summary>
    /// Sprites from the sheet are expected to be ordered top-left to bottom-right.
    /// </summary>
    public Biome(SpriteSheet sprites)
    {
        sprite = sprites;
    }

    public Biome(Sprite sprite)
    {
        this.sprite = sprite;
    }

    protected Biome() { }

    public virtual void GenerateTile(int x, int y, TileCell cell, PerlinNoise noise)
    {
        cell.PlaceNew(sprite.Source, sprite);
    }
}
