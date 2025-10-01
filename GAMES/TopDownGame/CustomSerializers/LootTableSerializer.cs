using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using TopDownGame.Loot;
using WinterRose.Monogame;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TopDownGame.CustomSerializers
{
    class LootTableSerializer : CustomValueProvider<LootTable>
    {
        public override LootTable? CreateObject(object value, WinterForgeVM executor) => AssetDatabase.LoadAsset<LootTable>((string)value);
        public override object CreateString(LootTable obj, ObjectSerializer serializer)
        {
            obj.Save();
            return obj.Name;
        }
    }
}
