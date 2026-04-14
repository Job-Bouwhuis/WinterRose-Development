namespace WinterRose.ForgeWarden.TileMaps;

public class PatternSheetTile : Tile
{
    private readonly SpriteSheet spriteSheet;
    private readonly int columns;
    private readonly int rows;

    public PatternSheetTile(SpriteSheet sheet, int columns, int layer = 0)
        : base(sheet.Source, sheet.GetSprite(0), layer)
    {
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns));

        spriteSheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        this.columns = columns;
        rows = Math.Max(1, (int)Math.Ceiling(sheet.SpriteCount / (float)columns));
    }

    public override void Draw(Matrix4x4 viewMatrix, Vector2 worldPos, Vector2 gridPos, int tileSize)
    {
        int gx = PositiveMod((int)gridPos.X, columns);
        int gy = PositiveMod((int)gridPos.Y, rows);

        int index = gy * columns + gx;
        if (index >= spriteSheet.SpriteCount)
            index %= spriteSheet.SpriteCount;

        Sprite = spriteSheet.GetSprite(index);
        base.Draw(viewMatrix, worldPos, gridPos, tileSize);
    }

    private static int PositiveMod(int value, int modulo)
    {
        int m = value % modulo;
        return m < 0 ? m + modulo : m;
    }
}
