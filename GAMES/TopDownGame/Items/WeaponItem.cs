using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Items;

internal class WeaponItem : InventoryItem<Weapon>
{
    public override Weapon Item { get; set; }
    public override string Name => Item.Name;
    public override string Description => Item.Description;
    public override Rarity Rarity => Item.Rarity;
    public override int StackLimit => 1;

    protected override void ConfigureClone(InventoryItem<Weapon> clone) { } // no configuration needed. the base clone is enough
}
