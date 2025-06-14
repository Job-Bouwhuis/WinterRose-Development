using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden
{
    public static class MathHelper
    {
        public static float LerpAngle(float from, float to, float t)
        {
            float delta = ((to - from + 180f) % 360f) - 180f;
            return from + delta * t;
        }
    }
}
