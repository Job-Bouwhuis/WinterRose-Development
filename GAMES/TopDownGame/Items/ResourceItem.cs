using TopDownGame.Inventories;
using TopDownGame.Resources;
using WinterRose.Monogame;

namespace TopDownGame.Items;
internal class ResourceItem : InventoryItem<Resource>
{
    public override Resource Item { get; set; }
    public override string Name => Item.Name;
    public override string Description => Item.Description;
    public override Rarity Rarity => Item.Rarity;
    public override int Count { get => Item.Amount; set => Item.Amount = value; }
    public override int StackLimit => Item.Timeout is null ? int.MaxValue : 1;

    protected override void ConfigureClone(InventoryItem<Resource> clone) => clone.Item = Item;
}
