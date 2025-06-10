using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Physics
{
    public class Collider : PhysicsComponent
    {
        public CollisionObject CollisionObject { get; private set; }
        public CollisionShape CollisionShape => CollisionObject.CollisionShape;

        public Collider(CollisionShape shape)
        {
            CollisionObject = new CollisionObject();
            CollisionObject.CollisionShape = shape;
        }

        public void Dispose()
        {
            CollisionObject?.Dispose();
            CollisionObject = null;
        }

        public override void Sync()
        {
            Matrix4x4 worldMatrix = transform.worldMatrix;
            if(CollisionShape is Box2DShape box2d)
                worldMatrix.Translation -= (box2d.HalfExtentsWithoutMargin / 2).ToNumerics();
            if(CollisionShape is BoxShape box)
                worldMatrix.Translation -= (box.HalfExtentsWithoutMargin / 2).ToNumerics();
            
            CollisionObject.WorldTransform = worldMatrix.ToBullet();

        }

        public override void AddToWorld(DiscreteDynamicsWorld physicsWorld)
        {
            physicsWorld.AddCollisionObject(CollisionObject);
            AddedToWorld = true;
        }

        public override void RemoveFromWorld(DiscreteDynamicsWorld physicsWorld)
        {
            physicsWorld.RemoveCollisionObject(CollisionObject);
            AddedToWorld = false;
        }
    }
}
