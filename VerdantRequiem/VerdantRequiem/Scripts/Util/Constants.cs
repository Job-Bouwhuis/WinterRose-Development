using System;
using System.Collections.Generic;
using System.Text;

namespace VerdantRequiem.Scripts.Util;

public static class Constants
{
    public class CollisionLayers
    {
        public const int PLAYER = 1 << 0;
        public const int ENEMY = 1 << 1;
        public const int WORLD = 1 << 2;
        public const int WORLD_SECOND = 1 << 3;
        public const int PROJECTILE_PLAYER = 1 << 4;
        public const int PROJECTILE_ENEMY = 1 << 5;
    }
}
