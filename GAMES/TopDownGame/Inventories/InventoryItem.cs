using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Items;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Inventories;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class InventoryItem<T>
{
    /// <summary>
    /// The storage of what this item carries.
    /// </summary>
    public abstract T Item { get; set; }

    /// <summary>
    /// The name of the item
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// The description of the item
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// The items rarity
    /// </summary>
    public abstract Rarity Rarity { get; }
}

/// <summary>
/// The same as <see cref="InventoryItem{T}"/> but by defaults has 'object' as implementation
/// </summary>
public abstract class InventoryItem() : InventoryItem<object>();

