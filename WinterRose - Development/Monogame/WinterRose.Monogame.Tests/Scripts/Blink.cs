using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests.Scripts
{
    internal class Blink : ObjectBehavior
    {
        [IncludeInTemplateCreation]
        public float Distance { get; set; } = 100;

        private void Update()
        {
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                if (Input.GetKey(Microsoft.Xna.Framework.Input.Keys.A))
                    transform.position = transform.position with { X = transform.position.X - Distance };
                if (Input.GetKey(Microsoft.Xna.Framework.Input.Keys.D))
                    transform.position = transform.position with { X = transform.position.X + Distance };

                if(transform.position.Y > 500)
                {  
                    transform.position = new(0, -100);
                }
            }
        }
    }
}
