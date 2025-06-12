using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Tests
{
    class ImportantComponent : Component
    {
        [InjectFromOwner]
        public Mover mover;

        [InjectFrom(EntityName = "cam")]
        public Camera cam;
    }
}
