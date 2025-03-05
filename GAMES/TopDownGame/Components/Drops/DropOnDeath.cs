using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Components.Loot;
using TopDownGame.Drops;
using TopDownGame.Inventories.Base;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;

namespace TopDownGame.Components.Drops
{
    [RequireComponent<Vitality>]
    class DropOnDeath : ObjectComponent
    {
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
