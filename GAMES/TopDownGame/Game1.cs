using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using TopDownGame.Levels;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;

namespace TopDownGame
{
    public class Game1 : Application
    {
        protected override World CreateWorld()
        {
            if (Debugger.IsAttached)
                Hirarchy.Show = true;
            return World.FromTemplate<Level1>();
        }
    }
}
