using System;
using WinterRose.Monogame;

namespace TopDownGame.Inventories.Base;
/// <summary>
/// Inventory Item classes implement this interface
/// </summary>
public interface IInventoryItem
{
    [Hide]
    Type ItemType { get; }
    [Show]
    string Name { get; }
    string Description { get; }
    [Show]
    Rarity Rarity { get; }
    [Show]
    int StackLimit { get; }
    [Show]
    int Count { get; set; }

    IInventoryItem? AddToStack(IInventoryItem item);
}
