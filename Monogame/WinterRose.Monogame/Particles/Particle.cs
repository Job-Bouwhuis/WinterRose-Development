using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WinterRose.Monogame.Particles
{
    public class Particle
    {
        [IncludeWithSerialization]
        public Vector2 position { get; set; }
        [IncludeWithSerialization]
        public ValueRange speed { get; set; }
        [IncludeWithSerialization]
        public Vector2 direction { get; set; }
        [IncludeWithSerialization]
        public ColorRange color { get; set; }
        [IncludeWithSerialization]
        public ValueRange scale { get; set; }
        [IncludeWithSerialization]
        public float rotation { get; set; }
        [IncludeWithSerialization]
        public float angularVelocity { get; set; }
        [IncludeWithSerialization]
        public float lifeTime
        {
            get => _lifeTime;
            set
            {
                if (!lifeTimeSet)
                {
                    lifeTimeSet = true;
                    totalLifeTime = value;
                }
                _lifeTime = value;
            }
        }
        bool lifeTimeSet = false;
        private float _lifeTime;
        private float totalLifeTime;
        public bool IsAlive => lifeTime > 0;

        public Particle()
        {
            direction = Vector2.Zero;
            color = new([(Color.White, 0), (Color.Transparent, 1)]);
            scale = new([(0, 1), (1, 0)]);
            rotation = 0;
            angularVelocity = 0;
            _lifeTime = 0;
            position = Vector2.Zero;
        }

        public void Update(float time)
        {
            if (!IsAlive) return;
            lifeTime -= time;
            float lifePercent = MathS.Clamp(lifeTime / totalLifeTime, 0, 1);
            // invert the lifePercent so that it goes from 0 to 1 instead of 1 to 0
            lifePercent = 1 - lifePercent;
            position += time * speed.GetValue(lifePercent) * direction;
            rotation += angularVelocity * time;
        }

        public void Render(SpriteBatch batch, Sprite sprite)
        {
            if (!IsAlive) return;
            float lifePercent = MathS.Clamp(lifeTime / totalLifeTime, 0, 1);
            // invert the lifePercent so that it goes from 0 to 1 instead of 1 to 0
            lifePercent = 1 - lifePercent;

            Color selectedcolor = color.GetColor(lifePercent);
            float fractionScale = scale.GetValue(lifePercent);

            batch.Draw(
                sprite, 
                position, 
                null, 
                selectedcolor, 
                rotation,
                new(sprite.Width * fractionScale / 2, sprite.Width * fractionScale / 2),
                fractionScale, 
                SpriteEffects.None, 
                1);
        }
    }
}
