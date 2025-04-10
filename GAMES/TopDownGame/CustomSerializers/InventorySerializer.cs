using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.CustomSerializers
{
    class InventorySerializer : CustomSerializer<Inventory>
    {
        public override object Deserialize(string data, int depth)
        {
            return AssetDatabase.LoadAsset<Inventory>(data);
        }

        public override string Serialize(object obj, int depth)
        {
            ((Inventory)obj).Save();
            return ((Inventory)obj).Name;
        }
    }
}
