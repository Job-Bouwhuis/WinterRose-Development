using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TopDownGame.Inventories.Base;
using TopDownGame.Loot;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.WinterForgeSerializing;

namespace TopDownGame.Inventories;

public sealed class Inventory : Asset
{
    [Show, WFInclude]
    public List<IInventoryItem> Items { get; private set; } = [];

    [WFExclude]
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
        while (toBeAdded.TryTake(out IInventoryItem? newItem))
        {
            foreach (IInventoryItem existingItem in Items)
            {
                if (existingItem.ItemType != newItem.ItemType)
                    continue;
                if (existingItem.Name != newItem.Name)
                    continue;

                var result = existingItem.AddToStack(newItem, out var remaining);
                if (remaining is null)
                    newItem = null;
                if (result is InventoryAditionResult.NewStack)
                {
                    toBeAdded.Add(remaining);
                    newItem = null;
                    break;
                }
                
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

    public override void Load()
    {
        Items = WinterForge.DeserializeFromFile<List<IInventoryItem>>(File.File.FullName);
    }

    public override void Save()
    {
        WinterForge.SerializeToFile(Items, File.File.FullName);
    }
    public override void Unload()
    {
        Save();
        Items.Clear();
    }
}
