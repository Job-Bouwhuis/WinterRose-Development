namespace WinterRose.ForgeWarden.TileMaps;

public class Tile
{
    public string Id { get; set; }
    public Sprite Sprite { get; set; }
    public int Layer { get; set; }
    public TileCell Cell { get; internal set; }

    internal bool Dirty { get; set; }

    public Tile(string id, Sprite sprite, int layer = 0)
    {
        Id = id;
        Sprite = sprite;
        Layer = layer;
        Dirty = false;
    }

    public virtual void Tick()
    {
    }

    public virtual void Draw(Matrix4x4 viewMatrix, Vector2 worldPos, Vector2 gridPos, int tileSize)
    {
        // default draw behaviour (same as before)
        if (Sprite == null) return;

        DrawSprite(Sprite, worldPos, tileSize);
    }

    protected void DrawSprite(Sprite sprite, Vector2 worldPos, int tileSize)
    {
        float half = tileSize * 0.5f;
        var dst = new Raylib_cs.Rectangle(worldPos.X - half, worldPos.Y - half, tileSize, tileSize);
        var origin = new Vector2(0, 0);
        Raylib_cs.Raylib.DrawTexturePro(sprite, sprite.SourceRect, dst, origin, 0f, new Raylib_cs.Color(255, 255, 255, 255));
    }

    public virtual void OnNeighborChanged(int changedX, int changedY)
    {
    }

    public virtual void OnPlaced(int x, int y)
    {
    }

    public virtual void OnRemoved(int x, int y)
    {
    }
}
