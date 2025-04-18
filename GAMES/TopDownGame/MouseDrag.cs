using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TopDownGame
{
    internal class MouseDrag : ObjectBehavior
    {
        protected override void Update()
        {
            Vector2 mousePos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);

            if (Input.MouseLeft)
                transform.position = mousePos;
        }
    }
}
