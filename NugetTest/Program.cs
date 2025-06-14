using BulletSharp;
using System.Reflection;
using WinterRose.FrostWarden;
using WinterRose.FrostWarden.Worlds;

namespace NugetTest;

internal class Program : Application
{
    static void Main(string[] args)
    {
        DiscreteDynamicsWorld world = new DiscreteDynamicsWorld(
            new CollisionDispatcher(new DefaultCollisionConfiguration()),
            new DbvtBroadphase(),
            new SequentialImpulseConstraintSolver(),
            new DefaultCollisionConfiguration());

        new Program().Run();
    }

    public override World CreateWorld()
    {
        return new World();
    }
}
