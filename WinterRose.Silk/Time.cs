using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.SilkEngine
{
    public static class Time
    {
        public static float deltaTime => sinceLastFrame * timeScale;
        public static float timeScale { get; set; } = 1;
        public static float unscaledDeltaTime => sinceLastFrame;
        internal static float sinceLastFrame { get; set; }
    }
}
