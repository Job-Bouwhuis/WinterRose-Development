using Microsoft.Xna.Framework;
using System;
using WinterRose.Serialization;

//int fibonacci(int value)
//{
//    if (value is 0 or 1)
//        return value;

//    return fibonacci(value - 1) + fibonacci(value - 2);
//}

//int seq = fibonacci(14);
//return;

using var game = new TopDownGame.Game1();
game.Run();

class Test
{
    public Test parent;
    public Test child;

    public int data;
}
