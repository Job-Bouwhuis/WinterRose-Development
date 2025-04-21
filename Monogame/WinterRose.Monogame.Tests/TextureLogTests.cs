using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.Tests
{
    [WorldTemplate]
    internal class TextureLogTests : WorldTemplate
    {
        public override void Build(in World world)
        {
            var obj = world.CreateObject("obj1");

            Sprite coolerAnimations = new Sprite("CoolerAnimations");
            SpriteSheet sheet = new SpriteSheet(coolerAnimations, 8, 8, 0, 0);
            Sprite testFile = new Sprite("testFile");
            Sprite trex = new Sprite("trex");


            obj.AddUpdateBehavior(x =>
            {
                if(!sheet.AreSpritesLoaded)
                {
                    return;
                }

                Debug.LogSprite(coolerAnimations);
                Debug.LogSprite(testFile);
                Debug.LogSprite(trex);
                foreach(int i in sheet.SpriteCount)
                {
                    Debug.LogSprite(sheet[i]);
                }
            });
        }
    }
}

