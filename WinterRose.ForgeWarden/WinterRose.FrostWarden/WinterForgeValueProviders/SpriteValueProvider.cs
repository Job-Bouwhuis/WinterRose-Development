using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.ForgeWarden.WinterForgeValueProviders
{
    class SpriteValueProvider : CustomValueProvider<Sprite>
    {
        public override Sprite? CreateObject(object value, WinterForgeVM executor)
        {
            return SpriteCache.Get((string)value);
        }
        public override object CreateString(Sprite obj, ObjectSerializer serializer)
        {
            return obj.Source;
        }
    }
}
