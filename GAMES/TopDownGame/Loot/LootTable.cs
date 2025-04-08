using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories.Base;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.Loot
{
    /// <summary>
    /// Represents a loot table that defines possible item drops and their chances.
    /// Use <see cref="WithName(string)"/> to retrieve a specific loot table by name.
    /// </summary>
    internal class LootTable<T> : Asset where T : class
    {
        [IncludeWithSerialization]
        public List<LootChance<T>> Table { get; private set; } = [];

        [DefaultArguments("")]
        private LootTable(string name) : base(name)
        {
        }

        public override void Load() => Table = SnowSerializer.Deserialize<LootTable<T>>(File.ReadContent(),
                new() { IncludeType = true }).Result.Table;

        public override void Unload() => Table.Clear();

        public override void Save() => File.WriteContent(SnowSerializer.Serialize(this,
                new() { IncludeType = true }), true);

        public static LootTable<T> WithName(string name)
        {
            if (!AssetDatabase.AssetExistsOfType(name, typeof(LootTable<T>)))
                return new LootTable<T>(name);

            return AssetDatabase.LoadAsset<LootTable<T>>(name);
        }

        public T Generate()
        {
            if (Table.Count == 0)
                return null;

            float totalWeight = Table.Sum(entry => entry.Weight);
            float roll = new Random().NextFloat(0, totalWeight);

            float currentWeight = 0;
            foreach (var entry in Table)
            {
                currentWeight += entry.Weight;
                if (roll <= currentWeight)
                    return entry.Item;
            }

            return Table.Last().Item; // fallback, should never be reached
        }

        public IInventoryItem[] GenerateMultiple(int count = 1)
        {
            if (count <= 0)
                return [];

            IInventoryItem[] items = new IInventoryItem[count];

            for (int i = 0; i < count; i++)
                items[i] = (IInventoryItem)Generate();

            return items;
        }

        internal void Add(params ReadOnlySpan<LootChance<T>> loot) => Table.AddRange(loot);
    }
}
