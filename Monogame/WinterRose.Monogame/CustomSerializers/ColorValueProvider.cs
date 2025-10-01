using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.Monogame.CustomSerializers
{
    class ColorValueProvider : CustomValueProvider<Color>
    {
        public override Color CreateObject(object value, WinterForgeVM executor) => new Color((uint)value);
        public override object CreateString(Color color, ObjectSerializer serializer) => color.PackedValue;
    }
}
