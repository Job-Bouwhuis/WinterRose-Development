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
[RequireComponent<SpriteRenderer>(AutoAdd = true)]
public class Button : ObjectBehavior
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
    private SpriteRenderer renderer;
    /// <summary>
    /// Invoked on click
    /// </summary>
    public Action OnClick { get; set; } = delegate { };
    /// <summary>
    /// The sprite used for the button. If not set before awake is called, a new blank sprite is created
    /// </summary>
    public Sprite sprite
    {
        get
        {
            renderer ??= FetchComponent<SpriteRenderer>();
            return renderer.Sprite;
        }
        set
        {
            renderer ??= FetchComponent<SpriteRenderer>();
            renderer.Sprite = value;
        }
    }
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
    /// The layer depth used when rendering the button (a value between 0 and 1)
    /// </summary>
    public float LayerDepth { get; set; } = 0.5f;
    /// <summary>
    /// The sprite effects used when rendering the button
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; }

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
        if(sprite.Width is 1 && 
            sprite.Height is 1 && 
            sprite.GetPixel(0, 0).A == 0)
        {
            sprite = MonoUtils.CreateTexture((int)(textSize.X + 15), (int)(textSize.Y + 15), "#FFFFFF");
        }
        if (LayerDepth is 1)
            LayerDepth = .99f;
        text.LayerDepth = LayerDepth + .01f;
        text.Color = Color.Black;
        colorRange = new([new ColorRangePoint(ButtonTints.Normal, 0), new ColorRangePoint(ButtonTints.Normal, 1)]);
        previousEndColor = colorRange.Points[^1].Color;
    }

    protected override void Update()
    {
        Universe.RequestRender = true;
        Vector2 mousePos;
        if (owner.RenderSpace == RenderSpace.World)
            mousePos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
        else
            mousePos = Input.MousePosition;

        renderer ??= FetchComponent<SpriteRenderer>()!;
        isHovering = renderer.Bounds.Contains(mousePos);
        if (currentColorFraction < 1)
            currentColorFraction += Time.deltaTime * ColorFadeSpeed;
        if(currentColorFraction > 1)
            currentColorFraction = 1;

        renderer.Tint = colorRange.GetColor(currentColorFraction);

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


}
