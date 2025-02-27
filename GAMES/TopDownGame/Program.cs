using Microsoft.Xna.Framework;
using System;
using TopDownGame.Inventories;
using TopDownGame.Items;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Serialization;

using var game = new TopDownGame.Game1();
game.Run();

class Test
{
    public Test parent;
    public Test child;

    public int data;
}
