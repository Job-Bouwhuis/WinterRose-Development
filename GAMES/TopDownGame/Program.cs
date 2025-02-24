using Microsoft.Xna.Framework;
using WinterRose.Serialization;

using var game = new TopDownGame.Game1();
game.Run();

class Test
{
    public Test parent;
    public Test child;

    public int data;
}
