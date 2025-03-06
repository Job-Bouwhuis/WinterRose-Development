using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace WinterRose.Monogame.UI;

/// <summary>
/// Text that can be rendered in the game world
/// </summary>
public class Text : UIRenderer
{
    public string text { get; set; }
    public Color color { get; set; }
    public SpriteFont Font { get; set; }
    public Vector2 PositionOffset { get; set; } = new();

    public override RectangleF Bounds
    {
        get
        {
            var size = Size;
            return new(size.X, size.Y, transform.position.X - size.X / 2, transform.position.Y - size.Y / 2);
        }
    }

    /// <summary>
    /// The size of the string based on the transform scale
    /// </summary>
    public Vector2 Size => Font.MeasureString(text) * transform.scale;
    /// <summary>
    /// The raw size of the text itself, unscaled by <see cref="Transform.scale"/>
    /// </summary>
    public Vector2 SizeRaw => Font.MeasureString(text);

    /// <summary>
    /// The sprite effects used when rendering the text
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; }
    /// <summary>
    /// The layerdepth used when rendering the text (a value between 0 and 1
    /// </summary>
    public float LayerDepth { get; set; } = 0.5f;

    public Text() : this("New Text", Color.White, MonoUtils.DefaultFont)
    {

    }

    public Text(string text, Color color, SpriteFont font)
    {
        this.text = text;
        this.color = color;
        this.Font = font;
    }

    public override void Render(SpriteBatch batch)
    {
        var size = Size;
        batch.DrawString(Font, text, transform.position + PositionOffset, color, transform.rotation, size / 2, transform.scale, SpriteEffects, LayerDepth);
    }

}
