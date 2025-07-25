﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame
{
    /// <summary>
    /// It renders text... what more can i say?
    /// </summary>
    /// <param name="text"></param>
    [method: DefaultArguments("New Text")]
    public class TextRenderer(string text) : Renderer
    {
        public override RectangleF Bounds
        {
            get
            {
                var size = Font.MeasureString(Text);
                return new RectangleF(size.X, size.Y, transform.position.X, transform.position.Y);
            }
        }

        public override TimeSpan DrawTime { get; protected set; }
        
        public SpriteFont Font { get; set; } = MonoUtils.DefaultFont;
        public Color Color { get; set; } = Color.White;
        public string Text { get; set; } = text;

        public override void Render(SpriteBatch batch)
        {
            //Vector2 center = new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
            batch.DrawString(Font, Text, transform.position, Color);
        }
    }
}
