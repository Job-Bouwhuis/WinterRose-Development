using BulletSharp;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Worlds;

namespace WinterRose.FrostWarden.Physics
{
    public class PhysicsDebugDrawer
    {
        private World world;

        public PhysicsDebugDrawer(World world)
        {
            this.world = world;
        }

        public void Draw(Matrix4x4 worldMatrix)
        {
            foreach (var collider in world.GetAll<Collider>())
            {
                var shape = collider.CollisionShape;
                var position = (worldMatrix.ToBullet() * collider.CollisionObject.WorldTransform).Origin;
                var posVec3 = new Vector3(position.X, position.Y, position.Z);

                switch (shape.ShapeType)
                {
                    case BroadphaseNativeType.SphereShape:
                        DrawSphereShape((SphereShape)shape, posVec3);
                        break;

                    case BroadphaseNativeType.BoxShape:
                        DrawBoxShape((BoxShape)shape, posVec3);
                        break;

                    default:
                        DrawUnknownShape(posVec3);
                        break;
                }
            }
        }

        private void DrawSphereShape(SphereShape sphere, Vector3 position)
        {
            float radius = sphere.Margin; // Bullet sphere shape's margin is the radius
            Raylib.DrawSphereWires(position, radius, 16, 16, Color.Red);
        }

        private void DrawBoxShape(BoxShape box, Vector3 position)
        {
            var halfExtents = box.HalfExtentsWithMargin;
            var size = new Vector3(halfExtents.X * 2, halfExtents.Y * 2, halfExtents.Z * 2);
            
            Raylib.DrawRectangleLines((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, Color.Blue);
        }

        private void DrawUnknownShape(Vector3 position)
        {
            Raylib.DrawCubeWires(position, 1f, 1f, 1f, Color.Yellow);
        }
    }
}
