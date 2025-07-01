using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.AssetPipeline;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.FrostWarden.WinterForgeValueProviders
{
    class SpriteValueProvider : CustomValueProvider<Sprite>
    {
        public override Sprite? CreateObject(string value, InstructionExecutor executor)
        {
            return SpriteCache.Get(value);
        }
        public override string CreateString(Sprite obj, ObjectSerializer serializer)
        {
            return $"\"{obj.Source}\"";
        }
    }
}
