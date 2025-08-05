using TopDownGame.Inventories.Base;
using WinterRose;

namespace TopDownGame.Loot
{
    
    public class LootChance(float weight, IInventoryItem item)
    {
        public IInventoryItem Item { get; private set; } = item;
        public float Weight { get; private set; } = weight;

        public int MinDrops { get; set; } = 1;
        public int MaxDrops { get; set; } = 1;

        private LootChance() : this(0, null!) { } // for serialization
        public LootChance(float weight, IInventoryItem item, int minDrops, int maxDrops)
            : this(weight, item)
        {
            MinDrops = minDrops;
            MaxDrops = maxDrops;
        }
    }
}
