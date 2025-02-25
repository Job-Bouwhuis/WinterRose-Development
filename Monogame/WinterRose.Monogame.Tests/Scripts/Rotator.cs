using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests
{
    internal class Rotator : ObjectBehavior
    {
        [IncludeInTemplateCreation]
        public float Speed { get; set; } = 0.001f;
        protected override void Update()
        {
            // rotate the object 360 degrees per second and scale it on its speed
            transform.rotation += 360f * (float)Time.SinceLastFrame * Speed;
        }
    }
}
