using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add<T>(T component) where T : Component
        {
            if (typeof(T) == typeof(Transform))
                throw new InvalidOperationException("Adding Transform component is not allowed");

            if (component is RigidBodyComponent rb)
            {
                RigidBodyComponent? existingRigidBodyComponent = Get<RigidBodyComponent>();
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
        }

        public void Remove<T>() where T : Component
        {
            if (typeof(T) == typeof(Transform))
                throw new InvalidOperationException("Removal of the Transform component is not allowed");
            components.Remove(typeof(T));
        }

        public T? Get<T>() where T : Component
        {
            if (components.TryGetValue(typeof(T), out var list) && list.Count > 0)
                return (T)list[0];
            return null;
        }

        public bool Has<T>() where T : Component
        {
            return components.TryGetValue(typeof(T), out var list) && list.Count > 0;
        }

        public IEnumerable<T> GetAll<T>() where T : class, IComponent
        {
            foreach (var (type, list) in components)
                if (type.IsAssignableTo(typeof(T)))
                    foreach (var comp in list)
                        yield return (comp as T)!;
        }

    }
}
