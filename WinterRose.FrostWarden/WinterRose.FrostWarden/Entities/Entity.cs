using System.Runtime.CompilerServices;
using WinterRose.ForgeGuardChecks;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.Worlds;
using WinterRose.Reflection;

namespace WinterRose.FrostWarden.Entities;

public class Entity
{
    public string Name { get; set; }
    public string[] Tags { get; set; } = [];

    public World world { get; internal set; }
    public Transform transform { get; private set; }

    internal bool addedToWorld = false;
    internal List<Component> _getComponents => components.SelectMany(kv => kv.Value).ToList();

    public Entity(string name)
    {
        Name = name;
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

        if (addedToWorld)
        {
            if (component is PhysicsComponent ph)
            {
                ph.AddToWorld(world.Physics);
                ph.Sync();
            }

            component.CallAwake();
            InjectIntoComponent(component);
            component.CallStart();
        }
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

    public Component GetComponent(Type t)
    {
        if (components.TryGetValue(t, out var list) && list.Count > 0)
            return list[0];
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

    internal void InjectIntoComponents(World world)
    {
        foreach (var (type, list) in components)
            foreach (var comp in list)
            {
                InjectIntoComponent(comp);
            }
    }

    private void InjectIntoComponent(Component c)
    {
        object? @null = null;
        object o = c;
        ReflectionHelper rh = new ReflectionHelper(ref o);
        var members = rh.GetMembers();

        foreach (var member in members)
        {
            if (member.GetAttribute<InjectFromOwnerAttribute>() is InjectFromOwnerAttribute ifo)
            {
                Component? componentToInject = c.owner.GetComponent(member.Type);
                Forge.Expect(componentToInject).Not.Null();
                member.SetValue(ref o, componentToInject);
            }
            else if (member.GetAttribute<InjectFromChildrenAttribute>() is InjectFromChildrenAttribute ifc)
            {
                Entity? targetEntity = null;

                // Search children of c.owner matching EntityName or Tags
                foreach (var child in c.owner.transform.Children)
                {
                    bool nameMatch = string.IsNullOrEmpty(ifc.EntityName) || child.owner.Name == ifc.EntityName;
                    bool tagsMatch = ifc.Tags == null || ifc.Tags.Length == 0 || ifc.Tags.All(tag => child.owner.Tags.Contains(tag));

                    if (nameMatch && tagsMatch)
                    {
                        targetEntity = child.owner;
                        break;
                    }
                }

                Forge.Expect(targetEntity).Not.Null();
                object? componentToInject = targetEntity.GetComponent(member.Type);
                Forge.Expect(componentToInject).Not.Null();
                member.SetValue(ref o, componentToInject);
            }
            else if (member.GetAttribute<InjectFromAttribute>() is InjectFromAttribute ifa)
            {
                Entity? targetEntity = null;

                foreach (var entity in c.owner.world._Entities)
                {
                    bool nameMatch = string.IsNullOrEmpty(ifa.EntityName) || entity.Name == ifa.EntityName;
                    bool tagsMatch = ifa.Tags == null || ifa.Tags.Length == 0 || ifa.Tags.All(tag => entity.Tags.Contains(tag));

                    if (nameMatch && tagsMatch)
                    {
                        targetEntity = entity;
                        break;
                    }
                }

                Forge.Expect(targetEntity).Not.Null();
                object? componentToInject = targetEntity.GetComponent(member.Type);
                Forge.Expect(componentToInject).Not.Null();
                member.SetValue(ref o, componentToInject);
            }
        }
    }
}
