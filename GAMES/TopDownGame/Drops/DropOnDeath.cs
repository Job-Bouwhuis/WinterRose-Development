using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories.Base;
using TopDownGame.Loot;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Serialization;

namespace TopDownGame.Drops
{
    [RequireComponent<Vitality>]
    class DropOnDeath : ObjectComponent
    {
        [IncludeWithSerialization]
        public LootTable LootTable { get; set; }

        protected override void Awake()
        {
            var vitals = FetchComponent<Vitality>();
            vitals.OnDeath += Vitals_OnDeath;
        }

        private void Vitals_OnDeath()
        {
            IInventoryItem item = LootTable.Generate();
            ItemDrop.Create(transform.position, item);
        }
    }
}
