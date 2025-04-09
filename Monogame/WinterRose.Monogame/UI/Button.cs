using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.UI;

[RequireComponent<Text>(AutoAdd = true)]
public class Button : ActiveRenderer
{
    /// <summary>
    /// The text element for this button
    /// </summary>
    public Text text
    {
        get
        {
            return _text ??= FetchComponent<Text>();
        }
    }
    private Text _text;
    /// <summary>
    /// Invoked on click
    /// </summary>
    public Action OnClick { get; set; } = delegate { };
    /// <summary>
    /// The sprite used for the button. If not set before awake is called, a new blank sprite is created
    /// </summary>
    public Sprite sprite { get; set; }
    /// <summary>
    /// Whether the button activates the moment the left mouse button is pressed, or when it is released
    /// </summary>
    public bool ActivateOnMouseDown { get; set; } = false;
    /// <summary>
    /// The speed of which the button changes color
    /// </summary>
    public float ColorFadeSpeed { get; set; } = 4f;

    /// <summary>
    /// The colors used for the different button states
    /// </summary>
    public ButtonTints ButtonTints { get; set; } = new()
    {
        Normal = Color.White,
        Hover = Color.Red,
        Clicked = Color.Yellow
    };

    /// <summary>
    /// The bounds of the button. Relies on having a <see cref="Sprite"/> If not manually assigned as sprite manually, this property will fail if called before Awake was called.
    /// </summary>
    public override RectangleF Bounds => 
        new RectangleF(
            sprite.Bounds.Width, 
            sprite.Bounds.Height, 
            transform.position.X - sprite.Bounds.Width / 2, 
            transform.position.Y - sprite.Bounds.Height / 2);

    /// <summary>
    /// The layer depth used when rendering the button (a value between 0 and 1)
    /// </summary>
    public float LayerDepth { get; set; } = 0.5f;
    /// <summary>
    /// The sprite effects used when rendering the button
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; }
    public override TimeSpan DrawTime { get; protected set; }

    [Show]
    private bool isHovering = false;
    [Show]
    private bool isClicked = false;
    [Show]
    private ColorRange colorRange;
    [Show]
    private Color previousEndColor;
    [Show]
    private float currentColorFraction = 1;

    private bool once = false;

    protected override void Awake()
    {
        var textSize = text.SizeRaw;
        sprite ??= MonoUtils.CreateTexture((int)(textSize.X + 15), (int)(textSize.Y + 15), "#FFFFFF");

        if (LayerDepth is 1)
            LayerDepth = .99f;
        text.LayerDepth = LayerDepth + .01f;
        text.color = Color.Black;
        colorRange = new([new ColorRangePoint(ButtonTints.Normal, 0), new ColorRangePoint(ButtonTints.Normal, 1)]);
        previousEndColor = colorRange.Points[^1].Color;
    }

    protected override void Update()
    {
        Universe.RequestRender = true; 
        Vector2 mousePos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
        isHovering = Bounds.Contains(mousePos);
        if (currentColorFraction < 1)
            currentColorFraction += Time.deltaTime * ColorFadeSpeed;
        if(currentColorFraction > 1)
            currentColorFraction = 1;

        if (isHovering)
        {
            if (Input.GetMouseDown(MouseButton.Left))
            {
                isClicked = true;
                if (ActivateOnMouseDown)
                    OnClick();
            }
            if (Input.GetMouseUp(MouseButton.Left))
            {
                isClicked = false;
                if (!ActivateOnMouseDown)
                    OnClick();
            }

            if(isClicked)
            {
                colorRange = new([new ColorRangePoint(colorRange.GetColor(currentColorFraction), 0), new ColorRangePoint(ButtonTints.Clicked, 1)]);
            }
            else
            {
                colorRange = new([new ColorRangePoint(colorRange.GetColor(currentColorFraction), 0), new ColorRangePoint(ButtonTints.Hover, 1)]);
            }
        }
        else
        {
            isClicked = false;
            colorRange = new([new ColorRangePoint(colorRange.GetColor(currentColorFraction), 0), new ColorRangePoint(ButtonTints.Normal, 1)]);
        }

        if (colorRange.Points[^1].Color != previousEndColor)
        {
            previousEndColor = colorRange.Points[^1].Color;
            currentColorFraction = 0;
        }
    }

    public override void Render(SpriteBatch batch)
    {
        Stopwatch w = Stopwatch.StartNew();

        // calculate center of sprite based on sprite size and scale
        Vector2 size = new(sprite.Bounds.Width * transform.scale.X, sprite.Bounds.Height * transform.scale.Y);
        Vector2 center = new(size.X / 2, size.Y / 2);

        //Color selectedColor = isClicked ? ButtonTints.Clicked : isHovering ? ButtonTints.Hover : ButtonTints.Normal;

        batch.Draw(sprite, transform.position, null, colorRange.GetColor(currentColorFraction),
            0, center, transform.scale, SpriteEffects, LayerDepth);

        w.Stop();
        DrawTime = w.Elapsed;
    }
}
