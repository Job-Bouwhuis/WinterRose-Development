using Microsoft.Xna.Framework;
using System.Collections.Generic;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame;

public class Physics
{
    public static Vector2 Gravity { get; set; } = new(1, 9.81f);

    public static List<Collider> OverlapCircle(Vector2 position, float radius)
    {
        List<WorldObject> nearObjects = Universe.CurrentWorld.WorldChunkGrid.GetChunkAt((Vector2I)position).GetNearObjects();
        List<Collider> colliders = [];

        foreach(var obj in nearObjects)
        {
            if(obj.TryFetchComponent(out Collider col) && Vector2.Distance(obj.transform.position, position) <= radius)
                colliders.Add(col);
        }
        return colliders;
    }
}