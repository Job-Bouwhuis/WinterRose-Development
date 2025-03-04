using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Components.Players;
using TopDownGame.Inventories;
using TopDownGame.Inventories.Base;
using WinterRose.Monogame;

namespace TopDownGame.Drops;

/// <summary>
/// This component checks for a collision with an object with the flag specified at <see cref="TargetFlag"/>
/// </summary>
[ParallelBehavior]
internal class ItemDrop : ObjectBehavior
{
    public string TargetFlag { get; set; } = "Player";
    public float PickupDistance { get; set; } = 50;
    public float FlyTowardsTargetDistance { get; set; } = 300;
    public float FlySpeed { get; set; } = 200;
    public required IInventoryItem Item { get; init; }

    public ItemDrop(IInventoryItem item) => Item = item;

    private WorldObject target;

    protected override void Awake()
    {
        target = transform.world.FindObjectWithFlag(TargetFlag);

        if (target == null)
            throw new InvalidOperationException("No object with {TargetFlag} found in the world!");

        if(!target.HasComponent<Player>())
            throw new InvalidOperationException("Object with {TargetFlag} does not have component Player");

    }

    protected override void Update()
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if(distance <= PickupDistance)
        {
            if (!target.TryFetchComponent(out Player p))
                throw new InvalidOperationException("Object with target tag does not have Player component.");

            p.Inventory.AddItem(Item);
            Destroy(owner);
            return;
        }

        if(distance <= FlyTowardsTargetDistance)
        {
            var direction = -(transform.position - target.transform.position).Normalized();
            transform.Translate(FlySpeed * Time.SinceLastFrame * direction);
        }
    }
}
