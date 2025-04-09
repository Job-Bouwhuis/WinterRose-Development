using TopDownGame.Inventories.Base;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;
using WinterRose.Serialization;

namespace TopDownGame.Loot
{
    [IncludeAllProperties]
    public class LootChance<T>(float weight, T item) where T : class
    {
        [IncludeWithSerialization]
        public T Item { get; private set; } = item;
        [IncludeWithSerialization]
        public float Weight { get; private set; } = weight;

        public float MinDrops { get; set; } = 1;
        public float MaxDrops { get; set; } = 1;

        private LootChance() : this(0, null!) { } // for serialization
        public LootChance(float weight, T item,  float minDrops, float maxDrops)
            : this(weight, item)
        {
            MinDrops = minDrops;
            MaxDrops = maxDrops;
        }
    }
}
