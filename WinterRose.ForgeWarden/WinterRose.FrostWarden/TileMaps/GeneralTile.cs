using Raylib_cs;

namespace WinterRose.ForgeWarden.TileMaps;

public sealed class GeneralTile : Tile
{
    private readonly float rotation;

    public GeneralTile(string id, Sprite sprite, int layer, float rotation)
        : base(id, sprite, layer)
    {
        this.rotation = rotation;
    }

    public override void Draw(Matrix4x4 viewMatrix, Vector2 worldPos, Vector2 gridPos, int tileSize)
    {
        float half = tileSize * 0.5f;
        var dst = new Rectangle(worldPos.X - half, worldPos.Y - half, tileSize, tileSize);
        Raylib.DrawTexturePro(Sprite, Sprite.SourceRect, dst, new Vector2(half, half), rotation, Color.White);
    }
}