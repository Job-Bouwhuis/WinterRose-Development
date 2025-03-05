using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A square collider
    /// </summary>
    public sealed class SquareCollider : Collider
    {
        public SquareCollider() { }
        /// <summary>
        /// Creates a new <see cref="SquareCollider"/> with the given hitbox"/>
        /// </summary>
        /// <param name="hitbox"></param>
        public SquareCollider(Rectangle hitbox) : this() => Bounds = hitbox;
        /// <summary>
        /// Creates a new <see cref="SquareCollider"/> with the given <paramref name="renderer"/>s bounds and offset
        /// /// </summary>
        /// <param name="renderer"></param>
        public SquareCollider(Renderer renderer) : this()
        {
            Bounds = new Rectangle(0, 0, (renderer.Bounds.Width * renderer.transform.scale.X).FloorToInt(), (renderer.Bounds.Height * renderer.transform.scale.Y).FloorToInt());
            if (renderer is SpriteRenderer sp)
                HitboxOffset = (Vector2I)(sp.Origin * (Vector2I)Bounds.Size);
        }

        /// <summary>
        /// If the collider is touching the top of another collider
        /// </summary>
        [IgnoreInTemplateCreation]
        public bool top = false;
        /// <summary>
        /// If the collider is touching the bottom of another collider
        /// </summary>
        [IgnoreInTemplateCreation]
        public bool bottom = false;
        /// <summary>
        /// If the collider is touching the left of another collider
        /// </summary>
        [IgnoreInTemplateCreation]
        public bool left = false;
        /// <summary>
        /// If the collider is touching the right of another collider
        /// </summary>
        [IgnoreInTemplateCreation]
        public bool right = false;

        protected override void Awake()
        {
            if(TryFetchComponent<Renderer>(out var renderer))
            {
                Bounds = new Rectangle(0, 0, (renderer.Bounds.Width * renderer.transform.scale.X).FloorToInt(), (renderer.Bounds.Height * renderer.transform.scale.Y).FloorToInt());
                if (renderer is SpriteRenderer sp)
                    HitboxOffset = (Vector2I)(sp.Origin * (Vector2I)Bounds.Size);
            }
        }

        /// <summary>
        /// The number of collisions the collider is currently in
        /// </summary>
        public int collisions => pastColliders.Count;

        [IncludeWithSerialization]
        public List<string> IgnoredFlags { get; set; } = [];

        protected override List<CollisionInfo> CheckCollision(List<Collider> colliders)
        {
            List<CollisionInfo> collisions = new List<CollisionInfo>();
            foreach (var other in colliders)
            {
                if (other == this) continue;
                if (IgnoredFlags.Contains(other.owner.Flag))
                    continue;
                right = IsTouchingLeft(other);
                left = IsTouchingRight(other);
                bottom = IsTouchingTop(other);
                top = IsTouchingBottom(other);

                CollisionSide side = CollisionSide.None;
                if (right)
                    side |= CollisionSide.Right;
                if (left)
                    side |= CollisionSide.Left;
                if (top)
                    side |= CollisionSide.Top;
                if (bottom)
                    side |= CollisionSide.Bottom;

                if (left || right || top || bottom)
                    collisions.Add(new CollisionInfo(this, other, pastColliders.Any(x => x.other == other) ? CollisionType.Stay : CollisionType.Enter, side));
            }

            foreach (var col in pastColliders)
            {
                if (!collisions.Any(x => x.other == col.other))
                    collisions.Add(new(this, col.other, CollisionType.Exit, CollisionSide.None));
            }
            return collisions;
        }

        /// <summary>
        /// Whether or not the collider is touching <paramref name="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Intersects(Collider other)
        {
            return Bounds.Intersects(other.Bounds);
        }
        /// <summary>
        /// Gets the <see cref="CollisionInfo"/> for <paramref name="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override CollisionInfo GetCollisionInfo(Collider other)
        {
            right = IsTouchingLeft(other);
            left = IsTouchingRight(other);
            bottom = IsTouchingTop(other);
            top = IsTouchingBottom(other);

            CollisionSide side = CollisionSide.None;
            if (right)
                side |= CollisionSide.Right;
            if (left)
                side |= CollisionSide.Left;
            if (top)
                side |= CollisionSide.Top;
            if (bottom)
                side |= CollisionSide.Bottom;


            return new CollisionInfo(this, other, pastColliders.Any(x => x.other == other) ? CollisionType.Stay : CollisionType.Enter, side);
        }

        private bool IsTouchingLeft(Collider other)
        {
            return Bounds.Right > other.Bounds.Left &&
                 Bounds.Left < other.Bounds.Left &&
                 Bounds.Bottom > other.Bounds.Top &&
                 Bounds.Top < other.Bounds.Bottom;
        }

        private bool IsTouchingRight(Collider other)
        {
            return Bounds.Left < other.Bounds.Right &&
                Bounds.Right > other.Bounds.Right &&
                Bounds.Bottom > other.Bounds.Top &&
                Bounds.Top < other.Bounds.Bottom;
        }
        private bool IsTouchingTop(Collider other)
        {
            return Bounds.Bottom > other.Bounds.Top &&
                Bounds.Top < other.Bounds.Top &&
                Bounds.Right > other.Bounds.Left &&
                Bounds.Left < other.Bounds.Right;
        }
        private bool IsTouchingBottom(Collider other)
        {
            return Bounds.Top < other.Bounds.Bottom &&
                Bounds.Bottom > other.Bounds.Bottom &&
                Bounds.Right > other.Bounds.Left &&
                Bounds.Left < other.Bounds.Right;
        }

        internal override ObjectComponent Clone(WorldObject newOwner)
        {
            SquareCollider clone = base.Clone(newOwner) as SquareCollider;
            clone.IgnoredFlags = [.. IgnoredFlags];
            return clone;
        }
    }
}
