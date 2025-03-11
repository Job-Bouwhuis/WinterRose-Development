using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.UI;

[RequireComponent<Text>]
public sealed class TextButton : UIRenderer
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

    public override RectangleF Bounds => text.Bounds;

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
        text.color = Color.White;
        colorRange = new([new ColorRangePoint(ButtonTints.Normal, 0), new ColorRangePoint(ButtonTints.Normal, 1)]);
        previousEndColor = colorRange.Points[^1].Color;
    }

    protected override void Update()
    {
        Universe.RequestRender = true;
        Vector2 mousePos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
        isHovering = text.Bounds.Contains(mousePos);
        if (currentColorFraction < 1)
            currentColorFraction += Time.deltaTime * ColorFadeSpeed;
        if (currentColorFraction > 1)
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

            if (isClicked)
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

        text.color = colorRange.GetColor(currentColorFraction);
    }

    public override void Render(SpriteBatch batch)
    {

    }
}
