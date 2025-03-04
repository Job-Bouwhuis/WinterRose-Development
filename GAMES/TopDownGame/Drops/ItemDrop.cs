using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using TopDownGame.Inventories.Base;
using WinterRose.Monogame;

namespace TopDownGame.Drops;
[RequireComponent<Collider>]
internal class ItemDrop : ObjectComponent
{
    public required IInventoryItem Item { get; init; }

    public ItemDrop(IInventoryItem item) => Item = item;

    protected override void Awake()
    {
        Collider c = FetchComponent<Collider>();
        c.OnCollisionEnter += C_OnCollisionEnter;
    }

    private void C_OnCollisionEnter(CollisionInfo obj)
    {
        WorldObject o = obj.other.owner;

        
    }
}
