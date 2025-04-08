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

        private LootChance() : this(0, null!) { } // for serialization
    }
}
