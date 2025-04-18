using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;

namespace TopDownGame.Levels
{
    class InventoryCreationLevel : WorldTemplate
    {
        public override void Build(in World world)
        {
            MonoUtils.TargetFramerate = 144;
            world.Name = "InvCreate";

            var player = world.CreateObject<SpriteRenderer>("Player", 50, 50, Color.Red);
            player.AttachComponent<ModifiablePlayerMovement>();

            var box = world.CreateObject<SpriteRenderer>("box", 50, 50, Color.Blue);
            box.AttachComponent<MouseDrag>();
            world.CreateObject<SmoothCameraFollow>("camera").Target = player.transform;

            var crystal = world.CreateObject<SpriteRenderer>
                ("crystal", new Sprite("crystal.png"));
            crystal.transform.position = new Vector2(200, 200);
            crystal.ClipMask = new Rectangle(50, 50, 200, 200);


        }
    }
}
