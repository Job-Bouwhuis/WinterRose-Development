using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests.Scripts;

internal class tempMover : ObjectBehavior
{
    private void Update()
    {
        transform.position += new Vector2(0, 2f);
    }
}
