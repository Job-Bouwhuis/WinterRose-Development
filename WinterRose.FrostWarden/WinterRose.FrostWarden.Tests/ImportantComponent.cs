using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tests
{
    class ImportantComponent : Component
    {
        [InjectFromParent]
        public Transform parent;

        [InjectFromSelf]
        public Mover mover;

        [InjectFrom(EntityName = "cam")]
        public Camera cam;

        [InjectFromChildren]
        public Transform MyKid;

        [InjectAsset("bigimg")]
        public Sprite sprite;
    }
}
