using TopDownGame.Inventories.Base;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;
using WinterRose.Serialization;

namespace TopDownGame.Loot
{
    [IncludeAllProperties]
    public class LootChance(float weight, IInventoryItem item)
    {
        [IncludeWithSerialization]
        public IInventoryItem Item { get; private set; } = item;
        [IncludeWithSerialization]
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
