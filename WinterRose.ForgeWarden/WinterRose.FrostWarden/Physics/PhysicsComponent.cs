using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Physics
{
    public abstract class PhysicsComponent : Component
    {
        public bool AddedToWorld { get; set; } = false;

        public abstract void Sync();

        public abstract void AddToWorld(DiscreteDynamicsWorld physicsWorld);
        public abstract void RemoveFromWorld(DiscreteDynamicsWorld physicsWorld);
        internal void AddToWorld(object physics) => throw new NotImplementedException();
    }
}
