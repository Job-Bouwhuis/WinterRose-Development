using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden
{
    public static class Time
    {
        internal static void Update()
        {
            startup += deltaTime;
        }
        private static float startup = 0;


        public static float sinceStartup => startup;
        public static float deltaTime => ray.GetFrameTime();
    }
}
