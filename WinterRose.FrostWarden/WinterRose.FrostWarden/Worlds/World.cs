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

        physicsWorld.Gravity = new BulletSharp.Math.Vector3(0, 9.81f, 0);
    }

    internal void InitializeWorld()
    {
        foreach(var e in entities)
        {
            e.CallAwake();
        }
    }


    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
        entity.world = this;
        entity.addedToWorld = true;

        var physics = entity.GetAllComponents<PhysicsComponent>();
        foreach (var p in physics)
            if(!p.AddedToWorld)
            {
                p.AddToWorld(Physics);
                p.Sync();
            }
    }

    public void RemoveEntity(Entity entity)
    {
        var physics = entity.GetAllComponents<PhysicsComponent>();
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
            foreach (var updatable in entity.GetAllComponents<IUpdatable>())
                updatable.Update();
        }
    }

    public void Draw(Matrix4x4 viewMatrix)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            foreach (var renderable in entity.GetAllComponents<IRenderable>())
                renderable.Draw(viewMatrix);
        }

        physicsDebugDrawer.Draw(viewMatrix);
    }

    public IEnumerable<T> GetAll<T>() where T : class, IComponent
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            foreach (var c in entity.GetAllComponents<T>())
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
