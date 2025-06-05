using Raylib_cs;
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
        public Sprite sprite;

        public SpriteRenderer(Sprite sprite)
        {
            this.sprite = sprite;
        }

        public FrostShader Shader { get; private set; }
        public void SetShader(FrostShader s)
        {
            Shader = s;
        }

        public void Draw(Matrix4x4 viewMatrix)
        {
            if (sprite == null) return;

            Shader?.Apply(sprite.Size);

            var matrix = owner.transform.worldMatrix;
            Vector3 transformedPos = Vector3.Transform(Vector3.Zero, viewMatrix * matrix);
            Vector2 position2D = new Vector2(transformedPos.X, transformedPos.Y);

            float rotationDegrees = owner.transform.rotation.Z * (180f / MathF.PI);
            Vector2 scale = new Vector2(owner.transform.scale.X, owner.transform.scale.Y);

            Raylib.DrawTexturePro(
                sprite.Texture,
                new Rectangle(0, 0, sprite.Width, sprite.Height),
                new Rectangle(position2D.X, position2D.Y, sprite.Width * scale.X, sprite.Height * scale.Y),
                new Vector2(sprite.Width / 2f, sprite.Height / 2f),
                rotationDegrees,
                Color.White
            );

            Shader?.End();
        }

    }

}
