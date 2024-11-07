using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

public class CircleCollider : Collider
{
    public float Radius { get; set; }
    public bool IsTrigger { get; set; }

    //private List<CircleCollider?> OtherColliders => 
    //    world.WorldChunkGrid.GetObjectsInAndAroundChunk(owner.ChunkPosition)
    //    .Where(worldObject => worldObject != owner)
    //    .Select(worldObject => worldObject.FetchComponent<CircleCollider>())
    //    .Where(collider => collider != null)
    //    .ToList();

    public bool NoSelfForce { get; set; }

    public PhysicsObject? physics => owner.FetchComponent<PhysicsObject>();

    private void Awake()
    {
        if (Radius == 0)
            Radius = 1;

        if (TryFetchComponent<SpriteRenderer>(out var renderer))
            Radius = renderer.Bounds.Width / 2;

        Bounds = new RectangleF(Radius, Radius, 0, 0);
    }
    private bool HandleOtherCircleCollider(CircleCollider? other, out CollisionSide side)
    {
        if (other == null)
        {
            side = CollisionSide.None;
            return false;
        }

        // check for collision with other circle colliders
        var distance = Vector2.Distance(transform.position, other.transform.position);
        var radiusSum = Radius + other.Radius;

        // resolve collision. move both objects away from each other half the distance of the overlap
        if (distance < radiusSum)
        {
            var overlap = radiusSum - distance;
            HandleCircleCollision(other, overlap);

            // calculate the side of the collision
            var direction = Vector2.Normalize(transform.position - other.transform.position);
            var angle = Math.Atan2(direction.Y, direction.X);
            side = angle switch
            {
                _ when angle >= -MathHelper.PiOver4 && angle <= MathHelper.PiOver4 => CollisionSide.Right,
                _ when angle >= MathHelper.PiOver4 && angle <= 3 * MathHelper.PiOver4 => CollisionSide.Top,
                _ when angle >= 3 * MathHelper.PiOver4 || angle <= -3 * MathHelper.PiOver4 => CollisionSide.Left,
                _ => CollisionSide.Bottom
            };
            return true;
        }
        side = CollisionSide.None;
        return false;
    }

    private void HandleCircleCollision(CircleCollider other, float overlap)
    {
        if (IsTrigger || other.IsTrigger)
            return;

        var direction = Vector2.Normalize(transform.position - other.transform.position);
        transform.position += direction * overlap / 2;
        other.transform.position -= direction * overlap / 2;

        // apply a force to the object when there is a collision with another object.
        // and apply the force to the other object in the opposite direction
        var force = direction * overlap / 2;

        other.physics?.ApplyForce(force * -600);

        if (NoSelfForce)
            return;
        physics?.ApplyForce(force * 600);

        if (physics == null)
        {
            // if the object does not have a physics object attached to it, move the object away from the other object
            transform.position += direction * overlap; // we move the object the complete overlap distance as we assume the other object is static.
        }
    }

    protected override List<CollisionInfo> CheckCollision(List<Collider> colliders)
    {
        List<CollisionInfo> infos = [];
        foreach (var other in colliders)
        {
            if(other == this)
                continue;
            if (other == null) continue;
            if (other is CircleCollider othercircle)
                if (HandleOtherCircleCollider(othercircle, out var side))
                {
                    if (GetPreviousCollisionWith(othercircle))
                        infos.Add(new CollisionInfo(this, othercircle, CollisionType.Enter, side));
                    else
                        infos.Add(new CollisionInfo(this, othercircle, CollisionType.Stay, side));
                }
                else
                    infos.Add(new CollisionInfo(this, other, CollisionType.Exit, CollisionSide.None));

            if (other is SquareCollider square)
                if (HandleSquareCollider(square, out var side))
                {
                    if (GetPreviousCollisionWith(square))
                        infos.Add(new CollisionInfo(this, square, CollisionType.Stay, side));
                    else
                        infos.Add(new CollisionInfo(this, square, CollisionType.Enter, side));
                }
                else
                {
                    if (GetPreviousCollisionWith(square))
                        infos.Add(new CollisionInfo(this, square, CollisionType.Exit, CollisionSide.None));
                }
        }

        return infos;
    }

    private bool HandleSquareCollider(SquareCollider square, out CollisionSide side)
    {
        // get the line segment of the square collider that is closes to the circle collider
        var squarePos = square.transform.position;
        var circlePos = transform.position;

        var closestPoint = new Vector2(
                       MathHelper.Clamp(circlePos.X, squarePos.X - square.Bounds.Width / 2, squarePos.X + square.Bounds.Width / 2),
                       MathHelper.Clamp(circlePos.Y, squarePos.Y - square.Bounds.Height / 2, squarePos.Y + square.Bounds.Height / 2));

        Debug.Log(circlePos);
        Debug.Log(closestPoint);

        // check if the circle collider is touching the square collider
        var distance = Vector2.Distance(circlePos, closestPoint);
        if (distance < Radius)
        {
            var overlap = Radius - distance;
            var direction = Vector2.Normalize(circlePos - closestPoint);
            transform.position += direction * overlap;

            // calculate the side of the collision
            var angle = Math.Atan2(direction.Y, direction.X);
            side = angle switch
            {
                _ when angle >= -MathHelper.PiOver4 && angle <= MathHelper.PiOver4 => CollisionSide.Right,
                _ when angle >= MathHelper.PiOver4 && angle <= 3 * MathHelper.PiOver4 => CollisionSide.Top,
                _ when angle >= 3 * MathHelper.PiOver4 || angle <= -3 * MathHelper.PiOver4 => CollisionSide.Left,
                _ => CollisionSide.Bottom
            };
            return true;
        }

        side = CollisionSide.None;
        return false;
    }

    public override bool Intersects(Collider other)
    {
        throw new NotImplementedException();
    }

    public override CollisionInfo GetCollisionInfo(Collider other)
    {
        throw new NotImplementedException();
    }
}