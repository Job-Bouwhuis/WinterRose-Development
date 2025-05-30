using System;
using WinterRose;
using WinterRose.Monogame;

namespace TopDownGame.Inventories.Base;
/// <summary>
/// Inventory Item classes implement this interface
/// </summary>
public interface IInventoryItem
{
    /// <summary>
    /// The system type of the item
    /// </summary>
    [Hide]
    Type ItemType { get; }

    /// <summary>
    /// The name of the item
    /// </summary>
    [Show]
    string Name { get; }

    /// <summary>
    /// A description about the item
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The rarity of the item
    /// </summary>
    [Show]
    Rarity Rarity { get; }

    /// <summary>
    /// The limit of how many items may be in one stack
    /// </summary>
    [Show]
    int StackLimit { get; }

    /// <summary>
    /// The amount of items currently in the stack
    /// </summary>
    [Show]
    int Count { get; set; }

    /// <summary>
    /// The sprite of the item
    /// </summary>
    [Show]
    Sprite ItemSprite { get; set; } 

    /// <summary>
    /// Adds another item to the stack
    /// </summary>
    /// <param name="item"></param>
    /// <returns>remaining items if the stack of this item is full</returns>
    InventoryAditionResult AddToStack(IInventoryItem item, out IInventoryItem? remaining);

    IInventoryItem Clone();
}
