namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.UserInterface;

/// <summary>
/// RichText element that renders a clickable button.
/// The button invokes a registered function only when clicked.
/// </summary>
public class RichButton : RichElement
{
    public string Label { get; set; }
    public string FunctionName { get; set; }
    public Dictionary<string, string> Arguments { get; set; } = new();
    
    private bool lastFrameClicked = false;
    private bool hasBeenClicked = false;

    public ClickStyle ClickBehavior { get; set; } = ClickStyle.Up;
    public bool NoGlobalColors { get; set; } = false;
    private bool pressStartedInside = false;

    public RichButton(string label, string functionName)
    {
        Label = label;
        FunctionName = functionName;
    }

    public RichButton(string label, string functionName, Dictionary<string, string> arguments)
    {
        Label = label;
        FunctionName = functionName;
        Arguments = arguments ?? new();
    }

    public override string ToString() => $"\\button[{Label};{FunctionName}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        // Measure button dimensions
        var labelSize = Raylib.MeasureTextEx(context.RichText.Font, Label, context.RichText.FontSize, context.RichText.Spacing);
        float padding = 2;
        float buttonWidth = labelSize.X + padding * 2;
        float buttonHeight = labelSize.Y + padding * 2;

        var buttonRect = new Rectangle(position.X, position.Y, buttonWidth, buttonHeight);

        // Determine colors (either from context.Style or derived from text color)
        Color backgroundColor;
        Color borderColor;
        Color textColor;
        Color hoverBackground;
        Color clickBackground;

        ContentStyle cs = context.Style;

        if (!NoGlobalColors)
        {
            backgroundColor = cs.ButtonBackground;
            borderColor = cs.ButtonBorder;
            textColor = cs.ButtonTextColor;
            hoverBackground = cs.ButtonHover;
            clickBackground = cs.ButtonClick;
        }
        else
        {
            // derive colors from overall text tint (make subtle UI-friendly backgrounds)
            Color baseText = context.OverallTint;

            // background is mostly white tinted by text color (soft tint)
            backgroundColor = ray.ColorLerp(baseText, new Color(255, 255, 255, 255), 0.92f);
            borderColor = ray.ColorLerp(baseText, new Color(0, 0, 0, 255), 0.6f);
            textColor = baseText;
            hoverBackground = ray.ColorLerp(backgroundColor, new Color(255, 255, 255, 255), 0.08f);
            clickBackground = ray.ColorLerp(backgroundColor, new Color(0, 0, 0, 255), 0.06f);
        }

        // Input / hover detection
        Vector2 mousePos = context.Input?.MousePosition ?? Vector2.Zero;
        bool isHover = Raylib.CheckCollisionPointRec(mousePos, buttonRect);

        // Track click/hover state
        bool isMouseDown = context.Input != null && Raylib.IsMouseButtonDown(MouseButton.Left);
        bool isClickedThisFrame = false;

        // Track press start for ClickStyle.Up
        if (context.Input != null)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                pressStartedInside = Raylib.CheckCollisionPointRec(context.Input.MousePosition, buttonRect);
            }

            if (ClickBehavior == ClickStyle.Down)
            {
                // Immediate activation on press
                if (Raylib.IsMouseButtonPressed(MouseButton.Left) && isHover)
                {
                    isClickedThisFrame = true;
                }
            }
            else // ClickStyle.Up
            {
                // Activation on release
                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    if (pressStartedInside && Raylib.CheckCollisionPointRec(context.Input.MousePosition, buttonRect))
                    {
                        isClickedThisFrame = true;
                    }
                    pressStartedInside = false;
                }
            }
        }

        // --- Draw background based on hover / mouse held state ---
        Color drawBg = backgroundColor;
        if ((ClickBehavior == ClickStyle.Up && pressStartedInside && isMouseDown) ||
            (ClickBehavior == ClickStyle.Down && isMouseDown && isHover))
        {
            drawBg = clickBackground; // held down visual
        }
        else if (isHover)
        {
            drawBg = hoverBackground;
        }

        // Draw button background & border
        Raylib.DrawRectangleRec(buttonRect, drawBg);
        Raylib.DrawRectangleLinesEx(buttonRect, 2, borderColor);
        buttonHeight = buttonRect.Height;

        // Draw button label
        Raylib.DrawTextEx(
            context.RichText.Font,
            Label,
            new Vector2(position.X + padding, position.Y + padding),
            context.RichText.FontSize,
            context.RichText.Spacing,
            textColor
        );

        // Action invocation (if clicked)
        if (isClickedThisFrame)
        {
            hasBeenClicked = true;

            if (context.AdditionalData.TryGetValue("FunctionRegistry", out var registryObj)
                && registryObj is FunctionRegistry registry)
            {
                var result = registry.InvokeFunction(FunctionName, Arguments, context, position);
                if (!result.Success)
                {
                    // optional: existing logging strategy
                }
            }
        }

        lastFrameClicked = isClickedThisFrame;

        return new RichTextRenderResult
        {
            WidthConsumed = buttonWidth + context.RichText.Spacing,
            HeightConsumed = buttonHeight
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        var labelSize = Raylib.MeasureTextEx(richText.Font, Label, richText.FontSize, richText.Spacing);
        float padding = 8;
        return labelSize.X + padding * 2 + richText.Spacing;
    }

    /// <summary>
    /// Check if this button has been clicked.
    /// Resets after checking.
    /// </summary>
    public bool GetAndResetClicked()
    {
        bool wasClicked = hasBeenClicked;
        hasBeenClicked = false;
        return wasClicked;
    }

    /// <summary>
    /// Check if button was clicked in the last frame without resetting.
    /// </summary>
    public bool WasClickedThisFrame => lastFrameClicked;
}
