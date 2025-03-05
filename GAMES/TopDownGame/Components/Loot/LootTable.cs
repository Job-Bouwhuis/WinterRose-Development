using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories.Base;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.Components.Loot
{
    [method: DefaultArguments("")]
    internal class LootTable(string name) : Asset(name)
    {
        [IncludeWithSerialization]
        public List<LootChance> Table { get; private set; } = [];

        public override void Load() => Table = SnowSerializer.Deserialize<LootTable>(File.ReadContent(),
                new() { IncludeType = true }).Result.Table;

        public override void Unload() => Table.Clear();

        public override void Save() => File.WriteContent(SnowSerializer.Serialize(this,
                new() { IncludeType = true }), true);

        public static LootTable WithName(string name)
        {
            if (!AssetDatabase.AssetExistsOfType(name, typeof(LootTable)))
                return new LootTable(name);

            return AssetDatabase.LoadAsset<LootTable>(name);
        }

        public IInventoryItem Generate()
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
                items[i] = Generate();

            return items;
        }

        internal void Add(params ReadOnlySpan<LootChance> loot) => Table.AddRange(loot);
    }
}
