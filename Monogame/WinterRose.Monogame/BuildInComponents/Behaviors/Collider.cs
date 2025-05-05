using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A base class for all colliders
    /// </summary>
    [ParallelBehavior]
    public abstract class Collider : ObjectBehavior
    {
        /// <summary>
        /// Invoked when the collider enters a collision
        /// </summary>
        public event Action<CollisionInfo> OnCollisionEnter = delegate { };
        /// <summary>
        /// Invoked when the collider exits a collision
        /// </summary>
        public event Action<CollisionInfo> OnCollisionExit = delegate { };
        /// <summary>
        /// Invoked when the collider stays in a collision
        /// </summary>
        public event Action<CollisionInfo> OnCollisionStay = delegate { };

        /// <summary>
        /// Whether or not the collider should allow overlapping or not
        /// </summary>
        [IncludeWithSerialization]
        public bool ResolveOverlaps { get; set; } = true;

        /// <summary>
        /// The hitbox of the collider
        /// </summary>
        [IncludeInTemplateCreation]
        public Rectangle Bounds { get; protected set; }
        /// <summary>
        /// The offset of the hitbox relative to the transform
        /// </summary>
        [IncludeInTemplateCreation]
        public Vector2I HitboxOffset { get; protected set; }

        [Show, IgnoreInTemplateCreation]
        internal List<CollisionInfo> pastColliders = new();

        public Collider() 
        { 
            Bounds = new Rectangle();
        }

        protected override void Update()
        {
            Bounds = Bounds with
            {
                X = transform.position.X.FloorToInt() - HitboxOffset.X,
                Y = transform.position.Y.FloorToInt() - HitboxOffset.Y,
            };
            List<Collider> colliders = new();

            foreach(var obj in Chunk.GetObjectsAroundObject(owner))
            {
                if (obj == null) continue;
                var cols = obj.FetchComponents<Collider>();
                colliders.AddRange(cols);
            }
            pastColliders = CheckCollision(colliders);
            pastColliders.Foreach(x =>
            {
                if(x.type == CollisionType.Enter)
                    OnCollisionEnter(x);
                if(x.type == CollisionType.Stay)
                    OnCollisionStay(x);
                if(x.type == CollisionType.Exit)
                    OnCollisionExit(x);
            });
            pastColliders = pastColliders.Where(x => x.type != CollisionType.Exit).ToList();
        }

        /// <summary>
        /// Implement this to check for collisions
        /// </summary>
        /// <param name="colliders"></param>
        /// <returns>Should return a list of <see cref="CollisionInfo"/> with all collisions, also those who are exiting the collision. <br></br>
        /// use <see cref="CollisionInfo.type"/> set to <see cref="CollisionType.Exit"/>
        /// </returns>
        protected abstract List<CollisionInfo> CheckCollision(List<Collider> colliders);
        /// <summary>
        /// When overriden, should return true if the collider intersects with the other collider
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool Intersects(Collider other);
        /// <summary>
        /// When overriden, should return a <see cref="CollisionInfo"/> with the collision info
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract CollisionInfo GetCollisionInfo(Collider other);
        /// <summary>
        /// Returns "Typename: <see cref="WorldObject.Name"/>"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}: {owner.Name}";
        }

        /// <summary>
        /// Get whether the collider collided with the other collider last frame
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool GetPreviousCollisionWith(Collider other)
        {
            return pastColliders.Any(x => x.other == other);
        }
    }

    /// <summary>
    /// Info about a collision
    /// </summary>
    /// <param name="me">The collider that created this info</param>
    /// <param name="other">The collider that was checked with</param>
    /// <param name="type">Enter, Say, or Exit</param>
    /// <param name="CollisionSide">Top, Down, Left, or Right</param>
    public record struct CollisionInfo(Collider me, Collider other, CollisionType type, CollisionSide CollisionSide);
    /// <summary>
    /// The type of collision
    /// </summary>
    public enum CollisionType
    {
        /// <summary>
        /// There is no collision
        /// </summary>
        None,
        /// <summary>
        /// The collider collided with another this frame
        /// </summary>
        Enter,
        /// <summary>
        /// The collider is colliding with another for multiple frames
        /// </summary>
        Stay,
        /// <summary>
        /// The collider was colliding with another last frame, but not this frame
        /// </summary>
        Exit
    }
    /// <summary>
    /// The side of the collision
    /// </summary>
    public enum CollisionSide
    {
        /// <summary>
        /// There is no collision
        /// </summary>
        None = 0,
        /// <summary>
        /// The collision is on the top
        /// </summary>
        Top,
        /// <summary>
        /// The collision is on the bottom
        /// </summary>
        Bottom,
        /// <summary>
        /// The collision is on the left
        /// </summary>
        Left,
        /// <summary>
        /// The collision is on the right
        /// </summary>
        Right,
    }

}
