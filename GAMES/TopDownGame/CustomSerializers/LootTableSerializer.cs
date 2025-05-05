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
        public override LootTable? CreateObject(string value, InstructionExecutor executor) => AssetDatabase.LoadAsset<LootTable>(value);
        public override string CreateString(LootTable obj, ObjectSerializer serializer)
        {
            obj.Save();
            return obj.Name;
        }
    }
}
