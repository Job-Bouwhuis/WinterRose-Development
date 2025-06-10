using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;
using Vector3 = BulletSharp.Math.Vector3;

namespace WinterRose.FrostWarden.Physics
{
    public class RigidBodyComponent : PhysicsComponent, IUpdatable
    {
        public RigidBody RigidBody { get; private set; }
        public float Mass { get; private set; }

        private MotionState motionState;

        public RigidBodyComponent(Collider collider, float mass)
        {
            Mass = mass;
            
            Vector3 localInertia = Vector3.Zero;
            if (Mass > 0f)
                collider.CollisionShape.CalculateLocalInertia(Mass, out localInertia);

            motionState = new DefaultMotionState();

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, motionState, collider.CollisionShape, localInertia);
            RigidBody = new RigidBody(rbInfo);
            RigidBody.Friction = 0.5f;
            RigidBody.Restitution = 0.1f;
            rbInfo.Dispose();
        }

        public void Update()
        {
            var trans = RigidBody.MotionState.WorldTransform;
            transform.Translate(trans);
        }

        public void Dispose()
        {
            RigidBody?.Dispose();
            motionState?.Dispose();
        }

        public override void Sync()
        {
            RigidBody.WorldTransform = transform.worldMatrix.ToBullet();
        }
        public override void AddToWorld(DiscreteDynamicsWorld physicsWorld)
        {
            physicsWorld.AddRigidBody(RigidBody);
            AddedToWorld = true;

            var dof = new Generic6DofConstraint(
                RigidBody,
                BulletSharp.Math.Matrix.Identity,
                false // world frame
);

            // ⛓ Lock movement along Z (i.e., keep it flat)
            dof.LinearLowerLimit = new BulletSharp.Math.Vector3(-float.MaxValue, -float.MaxValue, 0);
            dof.LinearUpperLimit = new BulletSharp.Math.Vector3(float.MaxValue, float.MaxValue, 0);

            // 🔒 Lock rotation around X and Y (so it doesn't flip or wobble), allow Z
            dof.AngularLowerLimit = new BulletSharp.Math.Vector3(0, 0, -float.MaxValue);
            dof.AngularUpperLimit = new BulletSharp.Math.Vector3(0, 0, float.MaxValue);

            // ✅ Finally, add the constraint to the world
            physicsWorld.AddConstraint(dof, disableCollisionsBetweenLinkedBodies: true);
        }

        public override void RemoveFromWorld(DiscreteDynamicsWorld physicsWorld)
        {
            physicsWorld.RemoveRigidBody(RigidBody);
            AddedToWorld = false;
        }
    }
}
