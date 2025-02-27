using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Inventories;
/// <summary>
/// Inventory Item classes implement this interface
/// </summary>
public interface IInventoryProperties
{
    Type ItemType { get; }
    string Name { get; }
    string Description { get; }
    Rarity Rarity { get; }
    int StackLimit { get; }
    int Count { get; set; }

    IInventoryProperties? AddToStack(IInventoryProperties item);
}
