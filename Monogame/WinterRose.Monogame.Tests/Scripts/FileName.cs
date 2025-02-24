using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests.Scripts
{
    internal class FileName : ObjectBehavior
    {
        protected override void Start()
        {
                MonoUtils.MainGame.Window.Title = "je dikke moeder";
        }

        protected override void Update() { }
    }
}
