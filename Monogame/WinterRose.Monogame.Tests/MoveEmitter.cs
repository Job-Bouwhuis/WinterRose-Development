using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests
{
    internal class MoveEmitter : ObjectBehavior
    {
        ParticleEmitter emitter;
        float time = 0;
        public float amplitude = 1;
        public float frequency = 1;

        public float startPoint = 0;

        public int burstEmit = 1000;

        private void Awake()
        {
            startPoint = transform.position.Y;
            emitter = FetchComponent<ParticleEmitter>();
        }

        private void Update()
        {
            time += Time.SinceLastFrame;
            var sin = MathF.Sin(time * frequency);
            var amped = sin * amplitude;
            var posed = startPoint + amped;
            transform.position = transform.position with { Y = posed };

            if (Input.SpaceDown)
            {
                emitter.Emit(burstEmit);
            }
        }
    }
}
