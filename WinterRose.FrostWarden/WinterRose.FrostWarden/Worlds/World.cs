using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;
using BulletSharp;
using WinterRose.FrostWarden.Physics;
using System.Collections.Concurrent;
using WinterRose.Reflection;
using WinterRose.ForgeGuardChecks;
using System.Diagnostics;

namespace WinterRose.FrostWarden.Worlds;

public class World : IDisposable
{
    public DiscreteDynamicsWorld Physics => physicsWorld;

    private readonly List<Entity> entities = new();
    internal IReadOnlyList<Entity> _Entities => entities; 

    private CollisionConfiguration collisionConfig;
    private CollisionDispatcher dispatcher;
    private BroadphaseInterface broadphase;
    private SequentialImpulseConstraintSolver solver;
    private DiscreteDynamicsWorld physicsWorld;
    private readonly ConcurrentBag<Action> deferredActions = new();

    public World()
    {
        collisionConfig = new DefaultCollisionConfiguration();
        dispatcher = new CollisionDispatcher(collisionConfig);
        broadphase = new DbvtBroadphase();
        solver = new SequentialImpulseConstraintSolver();
        physicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);

        physicsWorld.Gravity = new BulletSharp.Math.Vector3(0, 9.81f, 0);
    }

    public void Defer(Action action)
    {
        deferredActions.Add(action);
    }

    internal void InitializeWorld()
    {
        foreach(var e in entities)
        {
            e.CallAwake();
            e.InjectIntoComponents(this);

            e.CallStart();
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
        Defer(() =>
        {
            var physics = entity.GetAllComponents<PhysicsComponent>();
            foreach (var p in physics)
                p.RemoveFromWorld(Physics);

            entities.Remove(entity);
            entity.world = null;
            entity.addedToWorld = false;
        });
    }

    public void Update()
    {
        const float fixedTimeStep = 1f / 60f;
        physicsWorld.StepSimulation(Time.deltaTime, 10, fixedTimeStep);

        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            entity.CallUpdate();
        }

        for (int i = 0; i < deferredActions.Count; i++)
            if (deferredActions.TryTake(out Action? differedAction))
                differedAction?.Invoke();

        deferredActions.Clear();
    }

    public void Draw(Matrix4x4 viewMatrix)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity? entity = entities[i];
            entity.CallDraw(viewMatrix);
        }
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
