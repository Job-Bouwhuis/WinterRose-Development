using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Shaders;

namespace WinterRose.ForgeWarden
{
    public class SpriteRenderer : Component, IRenderable
    {
        [WFInclude]
        public Sprite Sprite { get; set; }

        public Color Tint { get; set; } = Color.White;

        public SpriteRenderer(Sprite sprite)
        {
            this.Sprite = sprite;
        }

        private SpriteRenderer() { } // for serialization

        [WFInclude]
        public ForgeShader? Shader { get; private set; }

        public void SetShader(ForgeShader s)
        {
            Shader = s;
        }

        public void Draw(Matrix4x4 viewMatrix)
        {
            if (Sprite == null) return;

            Shader?.Apply(Sprite.Size);

            var transform = owner.transform;

            Vector2 position2D = new Vector2(transform.position.X, transform.position.Y);

            Vector3 scale3D = transform.scale;
            Vector2 scale = new Vector2(scale3D.X, scale3D.Y);

            float rotationRadians = MathF.Atan2(
                2f * (transform.rotation.W * transform.rotation.Z),
                1f - 2f * (transform.rotation.Z * transform.rotation.Z)
            );

            float rotationDegrees = rotationRadians * (180f / MathF.PI);

            Vector2 origin = new Vector2(Sprite.Width / 2f, Sprite.Height / 2f);

            Raylib.DrawTexturePro(
                Sprite.Texture,
                new Rectangle(0, 0, Sprite.Width, Sprite.Height),
                new Rectangle(position2D.X, position2D.Y, Sprite.Width * scale.X, Sprite.Height * scale.Y),
                origin,
                rotationDegrees,
                Tint
            );

            Shader?.End();
        }

    }

}
