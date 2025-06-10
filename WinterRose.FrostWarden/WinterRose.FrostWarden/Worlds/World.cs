using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using BulletSharp;
using WinterRose.FrostWarden.Physics;

namespace WinterRose.FrostWarden.Worlds;

public class World : IDisposable
{
    private readonly List<Entity> entities = new();

    // Bullet physics core objects
    private CollisionConfiguration collisionConfig;
    private CollisionDispatcher dispatcher;
    private BroadphaseInterface broadphase;
    private SequentialImpulseConstraintSolver solver;
    private DiscreteDynamicsWorld physicsWorld;

    public DiscreteDynamicsWorld Physics => physicsWorld;

    private PhysicsDebugDrawer physicsDebugDrawer;

    public World()
    {
        collisionConfig = new DefaultCollisionConfiguration();
        dispatcher = new CollisionDispatcher(collisionConfig);
        broadphase = new DbvtBroadphase();
        solver = new SequentialImpulseConstraintSolver();
        physicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
        physicsDebugDrawer = new PhysicsDebugDrawer(this);

        // Set gravity (flip Y if needed depending on your coordinate system)
        physicsWorld.Gravity = new BulletSharp.Math.Vector3(0, 9.81f * 5, 0);

        // Your world-space rectangle bounds:
        Vector3 worldBoundsMin = new Vector3(0, 0, 0);
        Vector3 worldBoundsMax = new Vector3(Application.ScreenSize.x, Application.ScreenSize.y, 0.1f);

        // Thickness of the walls:
        float wallThickness = 10f;

        // Helper to create a static wall collider
        void AddWall(Vector3 position, Vector3 halfExtents)
        {
            var shape = new BoxShape(new BulletSharp.Math.Vector3(halfExtents.X, halfExtents.Y, halfExtents.Z));
            var motionState = new DefaultMotionState(BulletSharp.Math.Matrix.Identity);
            var rbInfo = new RigidBodyConstructionInfo(0, motionState, shape, BulletSharp.Math.Vector3.Zero);
            var body = new RigidBody(rbInfo);
            body.WorldTransform = BulletSharp.Math.Matrix.Translation(position.ToBullet());
            physicsWorld.AddRigidBody(body);
            rbInfo.Dispose();
        }

        // Bottom wall (along X axis, thin Y thickness)
        AddWall(
            new Vector3((worldBoundsMin.X + worldBoundsMax.X) / 2, worldBoundsMin.Y - wallThickness / 2, 0),
            new Vector3((worldBoundsMax.X - worldBoundsMin.X) / 2, wallThickness / 2, 1)
        );

        // Top wall
        AddWall(
            new Vector3((worldBoundsMin.X + worldBoundsMax.X) / 2, worldBoundsMax.Y + wallThickness / 2, 0),
            new Vector3((worldBoundsMax.X - worldBoundsMin.X) / 2, wallThickness / 2, 1)
        );

        // Left wall (along Y axis, thin X thickness)
        AddWall(
            new Vector3(worldBoundsMin.X - wallThickness / 2, (worldBoundsMin.Y + worldBoundsMax.Y) / 2, 0),
            new Vector3(wallThickness / 2, (worldBoundsMax.Y - worldBoundsMin.Y) / 2, 1)
        );

        // Right wall
        AddWall(
            new Vector3(worldBoundsMax.X + wallThickness / 2, (worldBoundsMin.Y + worldBoundsMax.Y) / 2, 0),
            new Vector3(wallThickness / 2, (worldBoundsMax.Y - worldBoundsMin.Y) / 2, 1)
        );
    }


    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
        entity.world = this;
        entity.addedToWorld = true;

        var physics = entity.GetAll<PhysicsComponent>();
        foreach (var p in physics)
            if(!p.AddedToWorld)
            {
                p.AddToWorld(Physics);
                p.Sync();
            }
    }

    public void RemoveEntity(Entity entity)
    {
        var physics = entity.GetAll<PhysicsComponent>();
        foreach (var p in physics)
            p.RemoveFromWorld(Physics);

        entities.Remove(entity);
        entity.world = null;
        entity.addedToWorld = false;
    }

    public void Update()
    {
        const float fixedTimeStep = 1f / 60f;
        // Step physics world — fixed timestep recommended, say 1/60f seconds, with max 10 substeps
        physicsWorld.StepSimulation(Time.deltaTime, 10, fixedTimeStep);

        // Then update entities
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            foreach (var updatable in entity.GetAll<IUpdatable>())
                updatable.Update();
        }
    }

    public void Draw(Matrix4x4 viewMatrix)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            foreach (var renderable in entity.GetAll<IRenderable>())
                renderable.Draw(viewMatrix);
        }

        physicsDebugDrawer.Draw(viewMatrix);
    }

    public IEnumerable<T> GetAll<T>() where T : class, IComponent
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            foreach (var c in entity.GetAll<T>())
                yield return c;
        }
    }

    public void Dispose()
    {
        // Clean up Bullet resources
        physicsWorld.Dispose();
        solver.Dispose();
        broadphase.Dispose();
        dispatcher.Dispose();
        collisionConfig.Dispose();
    }
}
