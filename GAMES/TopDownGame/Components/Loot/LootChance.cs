using TopDownGame.Inventories.Base;
using WinterRose.Serialization;

namespace TopDownGame.Components.Loot
{
    [IncludeAllProperties]
    public class LootChance(float weight, IInventoryItem item)
    {
        public float Weight { get; set; } = weight;
        public IInventoryItem Item { get; set; } = item;
        private LootChance() : this(0, null) { } // for serialization    
    }
}
