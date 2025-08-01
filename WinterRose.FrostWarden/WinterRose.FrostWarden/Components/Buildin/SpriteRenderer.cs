﻿using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Shaders;

namespace WinterRose.FrostWarden.Components
{
    public class SpriteRenderer : Component, IRenderable
    {
        [IncludeWithSerialization]
        public Sprite Sprite { get; set; }

        public SpriteRenderer(Sprite sprite)
        {
            this.Sprite = sprite;
        }

        private SpriteRenderer() { } // for serialization

        [IncludeWithSerialization]
        public FrostShader? Shader { get; private set; }

        public void SetShader(FrostShader s)
        {
            Shader = s;
        }

        public void Draw(Matrix4x4 viewMatrix)
        {
            if (Sprite == null) return;

            Shader?.Apply(Sprite.Size);

            var worldMatrix = owner.transform.worldMatrix;

            Matrix4x4 finalMatrix = worldMatrix * viewMatrix;

            Vector2 position2D = new Vector2(transform.position.X, transform.position.Y);

            float scaleX = new Vector2(finalMatrix.M11, finalMatrix.M12).Length();
            float scaleY = new Vector2(finalMatrix.M21, finalMatrix.M22).Length();
            Vector2 scale = new Vector2(scaleX, scaleY);

            float rotationRadians = MathF.Atan2(finalMatrix.M21, finalMatrix.M11);
            float rotationDegrees = rotationRadians * (180f / MathF.PI);

            Raylib.DrawTexturePro(
                Sprite.Texture,
                new Rectangle(0, 0, Sprite.Width, Sprite.Height),
                new Rectangle(position2D.X, position2D.Y, Sprite.Width * scale.X, Sprite.Height * scale.Y),
                new Vector2(Sprite.Width / 2f, Sprite.Height / 2f),
                rotationDegrees,
                Color.White
            );

            Shader?.End();
        }

    }

}
