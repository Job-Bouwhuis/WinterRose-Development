using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TopDownGame.Inventories.Base;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.Inventories;

public sealed class Inventory : Asset
{
    [Show, IncludeWithSerialization]
    public List<IInventoryItem> Items { get; private set; } = [];

    [ExcludeFromSerialization]
    private ConcurrentBag<IInventoryItem> toBeAdded = [];
    /// <summary>
    /// Creates a new Inventory asset. <br></br>Call <see cref="ValidateItemStacks"/> somewhere on the main thread game loop so that no duplicate item stacks remain
    /// </summary>
    /// <param name="assetName"></param>
    [DefaultArguments("")] // for serialization
    public Inventory(string assetName) : base(assetName) => Application.GameClosing += ExitHelper_GameClosing;

    private void ExitHelper_GameClosing() => Save();

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        toBeAdded.Add(item);
    }

    public void ValidateItemStacks()
    { 
        while(toBeAdded.TryTake(out IInventoryItem? newItem))
        {
            foreach (IInventoryItem existingItem in Items)
            {
                if (existingItem.ItemType != newItem.ItemType)
                    continue;
                if (existingItem.Name != newItem.Name)
                    continue;

                while (newItem != null)
                    newItem = existingItem.AddToStack(newItem);
                break;
            }

            if (newItem != null)
            {
                if (newItem.Count is 0)
                    return;
                Items.Add(newItem);
            }
        }
    }

    private static SerializerSettings inventorySaveSettings = new()
    {
        IncludeType = true,
        CircleReferencesEnabled = true
    };
    public override void Load() => Items = SnowSerializer.Deserialize<Inventory>(File.ReadContent(), inventorySaveSettings).Result.Items;
    public override void Save()
    {
        ValidateItemStacks();
        File.WriteContent(SnowSerializer.Serialize(this, inventorySaveSettings).Result, true);
    }
    public override void Unload()
    {
        Save();
        Items.Clear();
    }
}
