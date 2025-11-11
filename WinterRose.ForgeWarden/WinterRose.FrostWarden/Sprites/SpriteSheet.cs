using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
public class SpriteSheet : Sprite
{
    private readonly List<Rectangle> regions = new();
    public IReadOnlyList<Rectangle> Regions => regions;

    public override Texture2D Texture => base.Texture; // all share the same texture
    public override Vector2 Size => new Vector2(Texture.Width, Texture.Height);

    public int SpriteCount => regions.Count;

    public int Index { get; private set; }

    private SpriteSheet(Texture2D texture, List<Rectangle> regions)
    {
        this.Source = "GeneratedSpritesheet"; // or keep as parameter if needed
        this.Texture = texture;
        this.regions = regions;
        this.Index = 0;
    }

    public static SpriteSheet Load(string path, int cellWidth, int cellHeight, int padding = 0)
    {
        if (cellWidth <= 0 || cellHeight <= 0)
            throw new ArgumentException("Cell dimensions must be positive.");

        Texture2D texture = Raylib.LoadTexture(path);
        Raylib.SetTextureFilter(texture, TextureFilter.Point);

        int cols = (texture.Width + padding) / (cellWidth + padding);
        int rows = (texture.Height + padding) / (cellHeight + padding);

        var regions = new List<Rectangle>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int posX = x * (cellWidth + padding);
                int posY = y * (cellHeight + padding);
                if (posX + cellWidth > texture.Width || posY + cellHeight > texture.Height)
                    continue;

                regions.Add(new Rectangle(posX, posY, cellWidth, cellHeight));
            }
        }

        var sheet = new SpriteSheet(texture, regions);
        SpriteCache.RegisterTexture2D(path, texture);
        SpriteCache.RegisterSprite(sheet);
        return sheet;
    }

    public Sprite GetSprite(int index)
    {
        if (index < 0 || index >= regions.Count)
            throw new IndexOutOfRangeException($"Sprite index {index} out of range.");

        var rect = regions[index];
        return new Sprite(Texture, false)
        {
            Source = Source,
            SourceRect = rect
        };
    }

    public void SetIndex(int index)
    {
        if (index < 0 || index >= regions.Count)
            throw new IndexOutOfRangeException($"Sprite index {index} out of range.");
        Index = index;
        SourceRect = regions[index];
    }
}
