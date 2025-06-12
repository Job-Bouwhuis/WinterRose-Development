using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.Worlds;

namespace WinterRose.FrostWarden.Entities
{
    public class Entity
    {
        public World world { get; internal set; }
        public Transform transform { get; private set; }

        internal bool addedToWorld = false;

        public Entity()
        {
            transform = new Transform();
            transform.owner = this;
            components.Add(typeof(Transform), [transform]);
        }
        private readonly Dictionary<Type, List<Component>> components = new();

        private readonly List<IUpdatable> updatables = new();
        private readonly List<IRenderable> drawables = new();

        public float updateTimeMs { get; private set; }
        public float drawTimeMs { get; private set; }

        internal void CallUpdate()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var updatable in updatables)
                updatable.Update();

            stopwatch.Stop();
            updateTimeMs = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
        }

        internal void CallDraw(Matrix4x4 viewMatrix)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var drawable in drawables)
                drawable.Draw(viewMatrix);

            stopwatch.Stop();
            drawTimeMs = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
        }

        internal void CallAwake()
        {
            foreach (var list in components.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CallAwake();
                }
            }
        }

        internal void CallStart()
        {
            foreach (var list in components.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CallStart();
                }
            }
        }

        internal void CallOnVanish()
        {
            foreach (var list in components.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CallOnVanish();
                }
            }
        }

        internal void CallOnDestroy()
        {
            foreach (var list in components.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CallOnDestroy();
                }
            }
        }


        public void AddComponent<T>(T component) where T : Component
        {
            if (typeof(T) == typeof(Transform))
                throw new InvalidOperationException("Adding Transform component is not allowed");

            if (component is RigidBodyComponent rb)
            {
                RigidBodyComponent? existingRigidBodyComponent = GetComponent<RigidBodyComponent>();
                ForgeGuardChecks.Forge.Expect(existingRigidBodyComponent).Null();
            }

            component.owner = this;

            if (component is PhysicsComponent ph)
            {
                if(addedToWorld)
                {
                    ph.AddToWorld(world.Physics);
                    ph.Sync();
                }
            }

            if (!components.TryGetValue(typeof(T), out var list))
            {
                list = new List<Component>();
                components[typeof(T)] = list;
            }
            

            list.Add(component);

            if (component is IUpdatable updatable)
                updatables.Add(updatable);
            if (component is IRenderable drawable)
                drawables.Add(drawable);
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            if (typeof(T) == typeof(Transform))
                throw new InvalidOperationException("Removal of the Transform component is not allowed");

            if (components.TryGetValue(typeof(T), out var list))
            {
                foreach (var component in list)
                {
                    if (component is IUpdatable updatable)
                        updatables.Remove(updatable);
                    if (component is IRenderable drawable)
                        drawables.Remove(drawable);
                }
                components.Remove(typeof(T));
            }
        }

        public T? GetComponent<T>() where T : Component, IComponent
        {
            if (components.TryGetValue(typeof(T), out var list) && list.Count > 0)
                return (T)list[0];
            return null;
        }

        public bool HasComponent<T>() where T : Component, IComponent
        {
            return components.TryGetValue(typeof(T), out var list) && list.Count > 0;
        }

        public IEnumerable<T> GetAllComponents<T>() where T : class, IComponent
        {
            foreach (var (type, list) in components)
                if (type.IsAssignableTo(typeof(T)))
                    foreach (var comp in list)
                        yield return Unsafe.As<T>(comp);
        }
    }
}
