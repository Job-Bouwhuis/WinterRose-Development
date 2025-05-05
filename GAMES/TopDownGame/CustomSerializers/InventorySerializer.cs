using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using WinterRose.Monogame;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TopDownGame.CustomSerializers
{
    class InventorySerializer : CustomValueProvider<Inventory>
    {
        public override Inventory? CreateObject(string value, InstructionExecutor executor) => AssetDatabase.LoadAsset<Inventory>(value);
        public override string CreateString(Inventory inv, ObjectSerializer serializer)
        {
            inv.Save();
            return inv.Name;
        }
    }
}
