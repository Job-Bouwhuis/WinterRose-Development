using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests
{
    internal class MouseDrag : ObjectBehavior
    {
        Vector2 lastMousePos = new();
        protected override void Update()
        {
            Vector2 mousePos = Transform.ScreenToWorldPos(Input.MousePosition, null);

            if (Input.MouseLeft)
            {
                transform.position += mousePos - lastMousePos;

                Rope rope = FetchComponent<Rope>();
                rope.Nodes[0].CurrentPosition = transform.position;
            }
            lastMousePos = mousePos;
        }
    }
}
