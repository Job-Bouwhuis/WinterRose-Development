using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using WinterRose.Monogame;

namespace TopDownGame.Components.Players
{
    public class Player : ObjectComponent
    {
        public Inventory Inventory { get; private set; }

        public Player(string name)
        {
            string invAssetName = name + "_Inv";

            if (!AssetDatabase.AssetExists(invAssetName))
                AssetDatabase.GetOrDeclareAsset<Inventory>(invAssetName);
            Inventory = AssetDatabase.LoadAsset<Inventory>(invAssetName);
        }
    }
}
