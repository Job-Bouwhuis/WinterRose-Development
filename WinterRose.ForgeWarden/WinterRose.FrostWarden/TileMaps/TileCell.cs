using System.Reflection.Metadata.Ecma335;

namespace WinterRose.ForgeWarden.TileMaps;

public class TileCell
{
    public Biome Biome { get; internal set; }
    public TileMap Map { get; internal set; }

    public readonly List<Tile> Tiles = new();

    public TileCell()
    {

    }

    public void AddTile(Tile tile)
    {
        int insertIndex = Tiles.Count;
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (tile.Layer < Tiles[i].Layer)
            {
                insertIndex = i;
                break;
            }
        }
        Tiles.Insert(insertIndex, tile);
    }

    public bool RemoveTile(Predicate<Tile> match)
    {
        for (int i = Tiles.Count - 1; i >= 0; i--)
        {
            if (match(Tiles[i]))
            {
                Tiles.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public Tile GetTopTile()
    {
        if (Tiles.Count == 0) return null;
        return Tiles[^1];
    }

    public Tile NewTile(string id, Sprite sprite, int layer = 0)
    {
        var res = sprite is SpriteSheet sheet
        ? new SheetTile(sheet, layer)
        : new Tile(id, sprite, layer);

        res.Cell = this;
        return res;
    }

    public void PlaceNew(string id, Sprite sprite, int layer = 0) => AddTile(NewTile(id, sprite, layer));
}
