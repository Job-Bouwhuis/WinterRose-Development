using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using Textw = WinterRose.Monogame.TextRendering.Text;
using TextAlignment = WinterRose.Monogame.TextRendering.TextAlignment;
using System.Windows.Forms;

namespace WinterRose.Monogame.UI;

/// <summary>
/// Text that can be rendered in the game world
/// </summary>
public class Text : Renderer
{
    public Textw text { get; set; }
    public Color color { get; set; }
    public Vector2 PositionOffset { get; set; } = new();

    public override RectangleF Bounds => text.CalculateBounds(transform.position);

    public SpriteFont Font
    {
        get => text[0].Font;
        set => text.Foreach(word => word.Font = value);
    }

    /// <summary>
    /// The size of the string based on the transform scale
    /// </summary>
    public Vector2 Size
    {
        get
        {
            var bounds = text.CalculateBounds(transform.position);
            return new Vector2(bounds.Width, bounds.Height) * transform.scale;
        }
    }
    /// <summary>
    /// The raw size of the text itself, unscaled by <see cref="Transform.scale"/>
    /// </summary>
    public Vector2 SizeRaw
    {
        get
        {
            var bounds = text.CalculateBounds(transform.position);
            return new Vector2(bounds.Width, bounds.Height);
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
        this.color = color;
        this.Font = font;
    }

    public override void Render(SpriteBatch batch)
    {
        var size = Size;
        batch.DrawText(text, new Vector2(0, 0), new RectangleF(new Vector2(0, 0), MonoUtils.ScreenSize), TextAlignment.Left);
    }
}
