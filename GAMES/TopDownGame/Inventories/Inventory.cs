using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.Inventories;

public sealed class Inventory(string assetName) : Asset(assetName)
{
    private List<IInventoryProperties> items = [];

    public void AddItem(IInventoryProperties item)
    {
        ArgumentNullException.ThrowIfNull(item);

        foreach (IInventoryProperties i in items)
        {
            if (i.ItemType != item.ItemType)
                continue;
            
            while(item != null)
            {
                var itm = item;
                item = i.AddToStack(itm);
            }
            break;
        }

        if(item != null)
            items.Add(item);
    }

    public override void Load() => items = SnowSerializer.Deserialize<Inventory>(File.ReadContent()).Result.items;
    public override void Save() => File.WriteContent(SnowSerializer.Serialize(this).Result);
    public override void Unload() => items.Clear();
}
