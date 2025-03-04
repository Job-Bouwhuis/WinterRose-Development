using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using TopDownGame.Resources;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Items;
internal class ResourceItem : InventoryItem<Resource>
{
    public override Resource Item { get; set; }
    public override string Name => Item.Name;
    public override string Description => Item.Description;
    public override Rarity Rarity => Item.Rarity;
    public override int StackLimit => Item.Timeout is null ? -1 : 1;

    protected override void ConfigureClone(InventoryItem<Resource> clone) => clone.Item = Item;
}
