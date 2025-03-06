using Microsoft.Xna.Framework;
using System;
using TopDownGame.Inventories.Base;
using TopDownGame.Players;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;

namespace TopDownGame.Drops;

/// <summary>
/// This component checks for a collision with an object with the flag specified at <see cref="TargetFlag"/>
/// </summary>
[ParallelBehavior]
internal class ItemDrop : ObjectBehavior
{
    private bool combined = false;

    public string TargetFlag { get; set; } = "Player";
    public float PickupDistance { get; set; } = 25;
    public float FlyTowardsTargetDistance { get; set; } = 300;
    public float FlySpeed { get; set; } = 200;
    public required IInventoryItem Item { get; init; }

    public ItemDrop(IInventoryItem item) => Item = item;

    private WorldObject target;

    private static Sprite noTextureItemPlaceholder = MonoUtils.CreateTexture(5, 5, new Color(255, 150, 255));

    public static void Create(Vector2 pos, IInventoryItem item)
    {
        WorldObject obj = WorldObject.CreateNew("ItemDrop_" + item.Name);
        obj.transform.position = pos;
        Sprite sprite = item.ItemSprite;
        sprite ??= noTextureItemPlaceholder;
        obj.AttachComponent<SpriteRenderer>(sprite);
        var result = obj.AttachComponent<ItemDrop>(item);
        Universe.CurrentWorld.Instantiate(obj);
    }

    public static void Create(Vector2 pos, IInventoryItem item, World world)
    {
        WorldObject obj = WorldObject.CreateNew("ItemDrop_" + item.Name);
        obj.transform.position = pos;
        Sprite sprite = item.ItemSprite;
        sprite ??= noTextureItemPlaceholder;
        obj.AttachComponent<SpriteRenderer>(sprite);
        var result = obj.AttachComponent<ItemDrop>(item);
        world.Instantiate(obj);
    }

    protected override void Awake()
    { 
        target = transform.world.FindObjectWithFlag(TargetFlag);

        if (target == null)
            throw new InvalidOperationException($"No object with {TargetFlag} found in the world!");

        if(!target.HasComponent<Player>())
            throw new InvalidOperationException($"Object with {TargetFlag} does not have component Player");

        // set scale to something that makes items always same size lmao
    }

    protected override void Update()
    {
        if (combined)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if(distance <= PickupDistance)
        {
            if (!target.TryFetchComponent(out Player p))
                throw new InvalidOperationException("Object with target tag does not have Player component.");

            p.Inventory.AddItem(Item);
            Destroy(owner);
            return;
        }

        if (distance <= FlyTowardsTargetDistance)
        {
            var direction = -(transform.position - target.transform.position).Normalized();
            float closenessFactor = 1 + (FlyTowardsTargetDistance - distance) / PickupDistance;

            transform.Translate(FlySpeed * Time.deltaTime * direction * closenessFactor);
        }

        // item merging disabled cause it doesnt work

        //var nearObjects = world.WorldChunkGrid.GetObjectsAroundObject(owner);
        //foreach(var obj in nearObjects)
        //{
        //    if (obj is null || obj == owner || obj.IsDestroyed)
        //        continue;
        //    if (!obj.TryFetchComponent(out ItemDrop otherDrop))
        //        continue;

            //    if (!otherDrop.Item.Equals(Item))
            //        continue; // not the same item as this drop.
            //    if (otherDrop.combined)
            //        continue; // other drop already combined with another, skip this one

            //    var direction = (otherDrop.transform.position - transform.position).Normalized();
            //    var dropDistance = Vector2.Distance(transform.position, otherDrop.transform.position);
            //    if(dropDistance < 5)
            //    {
            //        if (!otherDrop.Item.Equals(Item))
            //            continue;
            //        if (Item.AddToStack(otherDrop.Item) is null)
            //        {
            //            Summon(transform.position, Item);
            //            combined = true;
            //            otherDrop.combined = true;
            //            Destroy(owner);
            //            Destroy(otherDrop.owner);
            //        }
            //    }

            //    transform.Translate(30 * Time.deltaTime * direction);
            //}
    }
}
