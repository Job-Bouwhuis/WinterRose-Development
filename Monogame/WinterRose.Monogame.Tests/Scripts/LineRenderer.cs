using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace WinterRose.Monogame.Tests;


internal class LineRenderer : Renderer
{
    Vector2 target;
    Vector2? start;

    Texture2D? tex;

    public override RectangleF Bounds => RectangleF.Zero;

    [IncludeInTemplateCreation]
    public Vector2 Target
    {
        get => target;
        set => target = value;
    }
    [IncludeInTemplateCreation()]
    public Vector2 Start
    {
        get
        {
            return start ?? transform.position;
        }
        set
        {
            start = value;
        }
    }

    public int LineWidth { get; set; } = 5;

    Color col = Color.White;

    public Color LineColor
    {
        get => col;
        set
        {
            col = value;
            tex?.Dispose();
            tex = MonoUtils.CreateTexture(1, 1, col);
        }
    }

    public override TimeSpan DrawTime { get; protected set; } = TimeSpan.Zero;

    public LineRenderer() : this(new())
    {

    }

    public LineRenderer(Vector2 target)
    {
        this.target = target;
        LineColor = Color.Magenta;
    }

    public override void Render(SpriteBatch batch)
    {
        Primitives2D.DrawLine(Start, Target, LineColor, LineWidth);
    }
}