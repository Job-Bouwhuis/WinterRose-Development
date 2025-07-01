using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using WinterRose.ForgeGuardChecks;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Physics;
using WinterRose.FrostWarden.Worlds;
using WinterRose.Reflection;

namespace WinterRose.FrostWarden.Entities;

public class Entity
{
    [IncludeWithSerialization]
    public string Name { get; set; }
    [IncludeWithSerialization]
    public string[] Tags { get; set; } = [];
    [IncludeWithSerialization]
    public World world { get; internal set; }
    [IncludeWithSerialization]
    public Transform transform { get; private set; }

    internal bool addedToWorld = false;
    internal IReadOnlyList<Component> _getComponents => components;

    public Entity(string name)
    {
        Name = name;
        transform = new Transform();
        transform.owner = this;
        components.Add(transform);
    }

    private Entity() { } // for serialization

    [IncludeWithSerialization]
    private List<Component> components = new();
    [IncludeWithSerialization]
    private List<IUpdatable> updatables = new();
    [IncludeWithSerialization]
    private List<IRenderable> drawables = new();

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
        foreach (var comp in components)
            comp.CallAwake();
    }

    internal void CallStart()
    {
        foreach (var comp in components)
            comp.CallStart();
    }

    internal void CallOnVanish()
    {
        foreach (var comp in components)
            comp.CallOnVanish();
    }

    internal void CallOnDestroy()
    {
        foreach (var comp in components)
            comp.CallOnDestroy();
    }

    public T AddComponent<T>(params object[] args) where T : Component
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Adding Transform component is not allowed");
        T component = ActivatorExtra.CreateInstance<T>(args);
        return AddComponent(component);
    }
    public T AddComponent<T>(T component) where T : Component
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Adding Transform component is not allowed");

        if (component is RigidBodyComponent rb)
        {
            RigidBodyComponent? existingRigidBodyComponent = GetComponent<RigidBodyComponent>();
            ForgeGuardChecks.Forge.Expect(existingRigidBodyComponent).Null();
        }

        component.owner = this;

        components.Add(component);

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
            InjectDependanciesIntoComponent(component);
            component.CallStart();
        }

        return component;
    }
    public void RemoveComponent<T>() where T : IComponent
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Removal of the Transform component is not allowed");

        foreach (var component in components)
        {
            if (component is not T)
                continue;

            if (component is IUpdatable updatable)
                updatables.Remove(updatable);
            if (component is IRenderable drawable)
                drawables.Remove(drawable);

            components.Remove(component);
        }
        

    }
    public T? GetComponent<T>() where T : Component
    {
        foreach (var c in components)
            if (c is T tc)
                return tc;
        return null;
    }
    public Component GetComponent(Type t)
    {
        foreach (var c in components)
            if (c.GetType() == t)
                return c;
        return null;
    }
    public bool HasComponent<T>() where T : Component, IComponent
    {
        foreach (var c in components)
            if (c is T tc)
                return true;
        return false;
    }
    public IEnumerable<T> GetAllComponents<T>() where T : Component, IComponent
    {
        foreach (var comp in components)
            if (comp is T c)
                yield return c;
    }

    internal void InjectIntoComponents(World world)
    {
        foreach (var comp in components)
            InjectDependanciesIntoComponent(comp);
    }

    private void InjectDependanciesIntoComponent(Component c)
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

    public bool TryFetchComponent<T>([NotNullWhen(true)] out T? vitals) where T : Component
    {
        vitals = GetComponent<T>();
        return vitals is not null;
    }
}
