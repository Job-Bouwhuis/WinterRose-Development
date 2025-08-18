using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.DependencyInjection;
using WinterRose.ForgeWarden.DependencyInjection.Handlers;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Entities;

public class Entity
{
    [WFInclude]
    public string Name { get; set; }
    [WFInclude]
    public string[] Tags { get; set; } = [];
    [WFInclude]
    public World world { get; internal set; }
    [WFInclude]
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

    [WFInclude]
    private List<Component> components = new();
    [WFInclude]
    private List<IUpdatable> updatables = new();
    [WFInclude]
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
            InjectDependenciesIntoComponent(component);
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
            InjectDependenciesIntoComponent(comp);
    }

    private static readonly Dictionary<Type, IInjectionHandler> HANDLERS = new()
        {
            { typeof(InjectFromSelfAttribute), new InjectFromOwnerHandler() },
            { typeof(InjectFromChildrenAttribute), new InjectFromChildrenHandler() },
            { typeof(InjectFromAttribute), new InjectFromWorldHandler() },
            { typeof(InjectAssetAttribute), new InjectAssetHandler() },
            { typeof(InjectFromParentAttribute), new InjectFromParentHandler() }
        };


    private void InjectDependenciesIntoComponent(Component component)
    {
        var rh = new ReflectionHelper(component);
        var members = rh.GetMembers();

        foreach (var member in members)
        {
            foreach (var kvp in HANDLERS)
            {
                var attr = member.GetAttribute(kvp.Key);
                if (attr != null)
                {
                    IInjectionHandler handler = kvp.Value;
                    handler.Inject(component, member, attr);
                    break;
                }
            }
        }
    }


    public bool TryFetchComponent<T>([NotNullWhen(true)] out T? vitals) where T : Component
    {
        vitals = GetComponent<T>();
        return vitals is not null;
    }
}
