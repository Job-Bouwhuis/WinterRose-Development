using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using TopDownGame.Loot;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.CustomSerializers
{
    class LootTableSerializer : CustomSerializer<LootTable>
    {
        public override object Deserialize(string data, int depth)
        {
            return AssetDatabase.LoadAsset<LootTable>(data);
        }

        public override string Serialize(object obj, int depth)
        {
            ((LootTable)obj).Save();
            return ((LootTable)obj).Name;
        }
    }
}
