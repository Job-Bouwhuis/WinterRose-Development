using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Items;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Inventories;

/// <summary>
/// An item in the <see cref="Inventory"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class InventoryItem<T> : IInventoryProperties
{
    [Hide, IgnoreInTemplateCreation]
    Type itemType;
    [Hide, IgnoreInTemplateCreation]
    public Type ItemType => itemType ??= typeof(T);

    public InventoryItem() { }
    protected InventoryItem(InventoryItem<T> original)
    {
        Item = original.Item;
        Count = original.Count;
    }
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

    /// <summary>
    /// The amount of items in a single stack there is allowed to be.
    /// </summary>
    public abstract int StackLimit { get; }
    /// <summary>
    /// The amount of this item kind is inventory item represents
    /// </summary>
    public int Count { get; set; } = 0;

    /// <summary>
    /// Splits the InventoryItem instances into two, where the one returned has <paramref name="count"/> items, and the remaining are left in the orignal
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public InventoryItem<T> Take(int count)
    {
        if (count < Count)
            throw new ArgumentOutOfRangeException("count");

        Count -= count;
        var clone = Clone();
        clone.Count = count;
        return clone;
    }

    /// <summary>
    /// Adds the given <paramref name="item"/> to the this stack
    /// </summary>
    /// <param name="item"></param>
    /// <returns>null if all items were able to be stacked into this instance. 
    /// or returns the reference to <paramref name="item"/> when there are items left over, and need to be stacked into the next item instance. 
    /// Or to create a new instance</returns>
    public IInventoryProperties? AddToStack(IInventoryProperties item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Count == 0) return null;
        if (Count == StackLimit) return item;

        if(item.Count + Count > StackLimit)
        {
            int remaining = item.Count + Count - StackLimit;
            item.Count = remaining;
            Count = StackLimit;
            return item;
        }

        Count += item.Count;
        return null;
    }

    /// <summary>
    /// Makes an exact copy of this InventoryItem instance.
    /// </summary>
    /// <returns></returns>
    public InventoryItem<T> Clone()
    {
        Type t = GetType();
        var clone = ActivatorExtra.CreateInstance(t, this) as InventoryItem<T>;
        ConfigureClone(clone);
        return clone;
    }

    protected abstract void ConfigureClone(InventoryItem<T> clone);
    IInventoryProperties? IInventoryProperties.AddToStack(IInventoryProperties item) => throw new NotImplementedException();
}

/// <summary>
/// The same as <see cref="InventoryItem{T}"/> but by defaults has 'object' as implementation
/// </summary>
public abstract class InventoryItem() : InventoryItem<object>();

