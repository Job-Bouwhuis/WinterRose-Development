using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Monogame.CustomSerializers
{
    class ColorValueProvider : CustomValueProvider<Color>
    {
        public override Color CreateObject(string value, InstructionExecutor executor) => new Color(uint.Parse(value));
        public override string CreateString(Color color, ObjectSerializer serializer) => color.PackedValue.ToString();
    }
}
