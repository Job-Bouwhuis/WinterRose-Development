using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TopDownGame.Inventories.Base;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.Inventories;

public sealed class Inventory : Asset
{
    [Show, IncludeWithSerialization]
    private List<IInventoryItem> items = [];

    /// <summary>
    /// Creates a new Inventory asset
    /// </summary>
    /// <param name="assetName"></param>
    [DefaultArguments("")] // for serialization
    public Inventory(string assetName) : base(assetName) => Application.GameClosing += ExitHelper_GameClosing;

    private void ExitHelper_GameClosing() => Save();

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        foreach (IInventoryItem i in items)
        {
            if (i.ItemType != item.ItemType)
                continue;

            while (item != null)
            {
                var itm = item;
                item = i.AddToStack(itm);
            }
            break;
        }

        if (item != null)
        {
            if (item.Count is 0)
                return;
            items.Add(item);
        }
    }

    private static SerializerSettings inventorySaveSettings = new()
    {
        IncludeType = true,
        CircleReferencesEnabled = true
    };
    public override void Load() => items = SnowSerializer.Deserialize<Inventory>(File.ReadContent(), inventorySaveSettings).Result.items;
    public override void Save() => File.WriteContent(SnowSerializer.Serialize(this, inventorySaveSettings).Result, true);
    public override void Unload() => items.Clear();
}
