using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using System.Windows.Forms;

namespace WinterRose.Monogame.UI;

/// <summary>
/// Text that can be rendered in the game world
/// </summary>
public class Text : Renderer
{
    public string text { get; set; }
    public Color Color { get; set; }
    public SpriteFont Font { get; set; } = MonoUtils.DefaultFont;
    public Vector2 PositionOffset { get; set; } = new();

    public override RectangleF Bounds
    {
        get
        {
            var size = Size;
            var position = PositionOffset + transform.position;
            return new RectangleF(size.X, size.Y, position.X, position.Y);
        }
    }

    /// <summary>
    /// The size of the string based on the transform scale
    /// </summary>
    public Vector2 Size
    {
        get
        {
            var size = Font.MeasureString(text);
            return new Vector2(size.X, size.Y) * transform.scale;
        }
    }
    /// <summary>
    /// The raw size of the text itself, unscaled by <see cref="Transform.scale"/>
    /// </summary>
    public Vector2 SizeRaw
    {
        get
        {
            var size = Font.MeasureString(text);
            return new Vector2(size.X, size.Y);
        }
    }

    /// <summary>
    /// The sprite effects used when rendering the text
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; }
    /// <summary>
    /// The layerdepth used when rendering the text (a value between 0 and 1
    /// </summary>
    public float LayerDepth { get; set; } = 0.5f;
    public override TimeSpan DrawTime { get; protected set; }

    public Text() : this("New Text", Color.White, MonoUtils.DefaultFont)
    {

    }

    public Text(string text) : this(text, Color.White, MonoUtils.DefaultFont)
    {

    }

    public Text(string text, Color color) : this(text, color, MonoUtils.DefaultFont)
    {

    }

    public Text(string text, Color color, SpriteFont font)
    {
        this.text = text;
        this.Color = color;
        this.Font = font;
    }

    public override void Render(SpriteBatch batch)
    {
        batch.DrawString(Font,
                         text,
                         transform.position,
                         Color,
                         transform.rotation,
                         SizeRaw * 0.5f,
                         transform.scale,
                         SpriteEffects,
                         LayerDepth);
    }
}
