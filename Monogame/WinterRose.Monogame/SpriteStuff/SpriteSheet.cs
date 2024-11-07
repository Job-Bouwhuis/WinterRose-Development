using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

/// <summary>
/// A sheet of sprites that can be used to extract sprites from a single sprite
/// </summary>
public sealed class SpriteSheet
{
    /// <summary>
    /// The range of sprites that will be selected from the sprite sheet
    /// </summary>
    public Range SpriteSelectionRange { get; set; }
    /// <summary>
    /// The source sprite used to extract the sprites
    /// </summary>
    public Sprite SourceSprite => source;
    /// <summary>
    /// Indicator if <see cref="GetSprite(int, int)"/> or <see cref="this[int]"/> can be used
    /// </summary>
    public bool AreSpritesLoaded => spritesLoaded;
    /// <summary>
    /// the amount of rectangles defined. Should be the same as <see cref="SpriteCount"/>
    /// </summary>
    public int RectangleCount => rectangles.Length;
    /// <summary>
    /// The amount of sprites extracted. Should be the same as <see cref="RectangleCount"/>
    /// </summary>
    public int SpriteCount => sprites.Length;
    /// <summary>
    /// The width of each sprite in the spritesheet
    /// </summary>
    public object Width { get; internal set; }
    /// <summary>
    /// The Height of each sprite in the spriteSheet
    /// </summary>
    public object Height { get; internal set; }
    /// <summary>
    /// How many pixels to skip from the edges of the <see cref="SourceSprite"/>
    /// </summary>
    public object EdgeMargin { get; internal set; }
    /// <summary>
    /// How many pixels to skip between each sprite
    /// </summary>
    public object PaddingBetweenSprites { get; internal set; }

    private Rectangle[,] rectangles;
    private Sprite source;
    private Sprite[] sprites;
    private bool spritesLoaded = false;

    /// <summary>
    /// Creates a new empty sprite sheet
    /// </summary>
    public SpriteSheet()
    {
        rectangles = new Rectangle[0, 0];
        sprites = Array.Empty<Sprite>();
    }
    /// <summary>
    /// Creates a new SpriteSheet with the given parameters
    /// </summary>
    /// <param name="spritePath">The path to load the <see cref="SourceSprite"/> from</param>
    /// <param name="spriteWidth">The width of each sprite</param>
    /// <param name="spriteHeight">the height of each sprite</param>
    /// <param name="edgeMargin">the amount of pixels to skip on the edges of the <see cref="SourceSprite"/></param>
    /// <param name="paddingInBetweenSprites">The amount of pixels in between the sprites</param>
    /// <param name="validateEmptySprites">Whether to exclude any sprite that is completely transparent</param>
    /// <param name="selectionRange">the range of sprites you wish to include in this sprite sheet</param>
    public SpriteSheet(string spritePath,
                       int spriteWidth,
                       int spriteHeight,
                       int edgeMargin,
                       int paddingInBetweenSprites,
                       bool validateEmptySprites = false,
                       Range? selectionRange = null)
        : this(new Sprite(spritePath), spriteWidth, spriteHeight, edgeMargin, paddingInBetweenSprites, validateEmptySprites, selectionRange) { }
    /// <summary>
    /// Creates a new Spritesheet using <paramref name="sprites"/> as the list of sprites to use. this will make the Rectangle features of the <see cref="SpriteSheet"/> unusable
    /// </summary>
    /// <param name="sprites"></param>
    public SpriteSheet(IEnumerable<Sprite> sprites)
    {
        this.sprites = sprites.ToArray();
        spritesLoaded = true;
    }
    /// <summary>
    /// Creates a new SpriteSheet with the given parameters
    /// </summary>
    /// <param name="sheet">The sprite to use as the <see cref="SourceSprite"/></param>
    /// <param name="spriteWidth">The width of each sprite</param>
    /// <param name="spriteHeight">the height of each sprite</param>
    /// <param name="edgeMargin">the amount of pixels to skip on the edges of the <see cref="SourceSprite"/></param>
    /// <param name="paddingInBetweenSprites">The amount of pixels in between the sprites</param>
    /// <param name="validateEmptySprites">Whether to exclude any sprite that is completely transparent</param>
    /// <param name="selectionRange">the range of sprites you wish to include in this sprite sheet</param>
    public SpriteSheet(Sprite sheet,
                       int spriteWidth,
                       int spriteHeight,
                       int edgeMargin,
                       int paddingInBetweenSprites,
                       bool validateEmptySprites = false,
                       Range? selectionRange = null)
    {
        source = sheet;
        Width = spriteWidth;
        Height = spriteHeight;
        EdgeMargin = edgeMargin;
        PaddingBetweenSprites = paddingInBetweenSprites;

        // Validate input parameters
        if (spriteWidth <= 0 || spriteHeight <= 0)
            throw new ArgumentException("Sprite width and height must be greater than zero.");

        int rows = (sheet.Width - 2 * edgeMargin) / (spriteWidth + paddingInBetweenSprites);
        int cols = (sheet.Height - 2 * edgeMargin) / (spriteHeight + paddingInBetweenSprites);
        if (selectionRange is null)
            SpriteSelectionRange = ..(rows * cols);
        else
            SpriteSelectionRange = selectionRange.Value;

        if (validateEmptySprites)
        {
            // Check for empty sprites
            int totalSprites = rows * cols;
            int nonEmptySprites = 0;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = edgeMargin + col * (spriteWidth + paddingInBetweenSprites);
                    int y = edgeMargin + row * (spriteHeight + paddingInBetweenSprites);

                    // Check if the sprite contains any non-transparent pixels
                    bool isEmpty = IsSpriteEmpty(sheet, x, y, spriteWidth, spriteHeight);

                    if (!isEmpty)
                    {
                        nonEmptySprites++;
                    }
                }
            }

            if (nonEmptySprites != totalSprites)
            {
                throw new ArgumentException("Some sprites in the sheet are empty.");
            }
        }

        // Initialize the rectangles array
        rectangles = new Rectangle[rows, cols];

        // Populate the rectangles array
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = edgeMargin + col * (spriteWidth + paddingInBetweenSprites);
                int y = edgeMargin + row * (spriteHeight + paddingInBetweenSprites);

                rectangles[row, col] = new Rectangle(x, y, spriteWidth, spriteHeight);
            }
        }

        var rects = GetRectangles(SpriteSelectionRange);
        rectangles = rects.ConvertTo2D();

        // Initialize the sprites array with null values
        sprites = new Sprite[rectangles.Length];

        // Start loading sprites in the background
        LoadSpritesAsync(sheet, validateEmptySprites);
    }

    /// <summary>
    /// Gets the sprite at the given position in x y 
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Sprite GetSprite(int x, int y)
    {
        if (y < 0 || y >= rectangles.GetLength(0) || x < 0 || x >= rectangles.GetLength(1))
        {
            throw new ArgumentException("Invalid row or column index.");
        }

        return sprites[y * rectangles.GetLength(1) + x];
    }
    /// <summary>
    /// Gets the rectangle at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Rectangle? GetRectangle(int index)
    {
        if (index < 0)
            return null;
        int x = index % rectangles.GetLength(0);
        int y = index / rectangles.GetLength(1);

        Rectangle rect;
        try
        {
            rect = this[x, y].Value;
        }
        catch (NullReferenceException)
        {
            return null;
        }

        return rect;
    }
    /// <summary>
    /// Get the rectangles inside the given range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Rectangle[] GetRectangles(Range range)
    {
        List<Rectangle> rectangles = new();
        foreach (int i in range)
        {
            var rect = GetRectangle(i);
            if (rect is not null)
                rectangles.Add(rect.Value);
        }
        return rectangles.ToArray();
    }

    /// <summary>
    /// Get the sprite at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Sprite? this[int index]
    {
        get
        {
            if (!spritesLoaded)
                return null;
            if (index < 0)
                return null;
            return sprites[index];
        }
    }
    /// <summary>
    /// Get the rectangle at the given position
    /// </summary>
    /// <param name="x">row position</param>
    /// <param name="y">column position</param>
    /// <returns></returns>
    public Rectangle? this[int x, int y]
    {
        get
        {
            if (rectangles.Length is 0)
                return null;

            return rectangles[y, x];
        }
    }

    private async void LoadSpritesAsync(Sprite sheet, bool validateEmptySprites)
    {
        await Task.Run(() =>
        {
            for (int row = 0; row < rectangles.GetLength(0); row++)
                for (int col = 0; col < rectangles.GetLength(1); col++)
                    if (SpriteSelectionRange.Contains(col))
                    {
                        int x = rectangles[row, col].X;
                        int y = rectangles[row, col].Y;
                        int width = rectangles[row, col].Width;
                        int height = rectangles[row, col].Height;

                        Sprite sprite = new Sprite(sheet, new Rectangle(x, y, width, height));

                        if (validateEmptySprites)
                        {
                            if (!IsSpriteEmpty(sheet, x, y, width, height))
                                sprites[row * rectangles.GetLength(1) + col] = sprite;
                        }
                        else
                            sprites[row * rectangles.GetLength(1) + col] = sprite;
                    }
        });

        spritesLoaded = true;
    }
    private bool IsSpriteEmpty(Texture2D sheet, int x, int y, int width, int height)
    {
        // Check if the sprite contains any non-transparent pixels
        Color[] colors = new Color[width * height];
        sheet.GetData(0, new Rectangle(x, y, width, height), colors, 0, colors.Length);

        return colors.All(x => x.A is 0);
    }
}

