using Microsoft.Xna.Framework;
using System;
using TopDownGame.Inventories.Base;
using TopDownGame.Players;
using WinterRose;
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
    private static int count = 0;

    [IncludeWithSerialization]
    public string TargetFlag { get; set; } = "Player";

    [IncludeWithSerialization]
    public float PickupDistance { get; set; } = 25;

    [IncludeWithSerialization]
    public float FlyTowardsTargetDistance { get; set; } = 300;

    [IncludeWithSerialization]
    public float FlySpeed { get; set; } = 200;

    [IncludeWithSerialization]
    public required IInventoryItem Item { get; init; }

    private ItemDrop() { } // for serialization
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
        WorldObject obj = WorldObject.CreateNew("ItemDrop_" + item.Name + "_" + ++count);
        obj.transform.position = pos;
        Sprite sprite = item.ItemSprite;
        sprite ??= noTextureItemPlaceholder;
        obj.AttachComponent<SpriteRenderer>(sprite);
        var result = obj.AttachComponent<ItemDrop>(item);
        obj.IncludeWithSceneSerialization = true;
        world.InstantiateExact(obj);
    }

    protected override void Awake()
    { 
        target = transform.world.FindObjectWithFlag(TargetFlag);

        if (target == null)
            throw new InvalidOperationException($"No object with {TargetFlag} found in the world!");

        if(!target.HasComponent<Player>())
            throw new InvalidOperationException($"Object with {TargetFlag} does not have component Player");
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
    }
}
