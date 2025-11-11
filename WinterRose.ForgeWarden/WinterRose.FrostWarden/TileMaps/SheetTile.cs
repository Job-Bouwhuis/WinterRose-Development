using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.TileMaps;
public class SheetTile : Tile
{
    private readonly SpriteSheet spriteSheet;

    private int currentIndex;

    public SheetTile(SpriteSheet sheet, int layer = 0)
        : base(sheet.Source, sheet.GetSprite(4), layer) // start with center tile by default
    {
        this.spriteSheet = sheet;
    }

    public override void Draw(Matrix4x4 viewMatrix, Vector2 worldPos, Vector2 gridPos, int tileSize)
    {
        UpdateSpriteIndex((int)gridPos.X, (int)gridPos.Y);
        base.Draw(viewMatrix, worldPos, gridPos, tileSize);
    }

    private void UpdateSpriteIndex(int x, int y)
    {
        var map = Cell?.Map;
        if (map == null)
            return;

        bool left = map.GetBiomeAt(x - 1, y) == Cell.Biome;
        bool right = map.GetBiomeAt(x + 1, y) == Cell.Biome;
        bool up = map.GetBiomeAt(x, y - 1) == Cell.Biome;
        bool down = map.GetBiomeAt(x, y + 1) == Cell.Biome;

        int index = 4; // center default

        // same 3x3 layout you mentioned earlier
        if (!up && !left) index = 0;
        else if (!up && left && right) index = 1;
        else if (!up && !right) index = 2;
        else if (up && !left && down) index = 3;
        else if (up && left && right && down) index = 4;
        else if (up && !right && down) index = 5;
        else if (!down && !left) index = 6;
        else if (!down && left && right) index = 7;
        else if (!down && !right) index = 8;

        if (index != currentIndex)
        {
            currentIndex = index;
            Sprite = spriteSheet.GetSprite(index);
        }
    }
}
