using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using WinterRose.Monogame.Attributes;
using WinterRose.Monogame.Exceptions;
using WinterRose.Monogame.Internals;
using WinterRose.Monogame.UI;
using WinterRose.Monogame.Worlds;
using WinterRose.Reflection;

namespace WinterRose.Monogame;

/// <summary>
/// An object that exists within a <see cref="Worlds.World"/>
/// </summary>
[DebuggerDisplay("WorldObject: {Name}")]
public class WorldObject
{
    /// <summary>
    /// The name of this object
    /// </summary>
    [IncludeWithSerialization]
    public string Name { get; set; }
    /// <summary>
    /// The time it took to update all components of this object
    /// </summary>
    public TimeSpan UpdateTime => updateTime;
    /// <summary>
    /// The time it took to execute the Render methods from all Render type components
    /// </summary>
    public TimeSpan DrawTime => drawTime;
    /// <summary>
    /// The transform of this object
    /// </summary>
    public Transform transform => _transform;
    /// <summary>
    /// Whether this object is active or not
    /// </summary>
    [IncludeWithSerialization]
    public bool IsActive
    {
        get
        {
            if (!_isActive)
                return _isActive;
            if (transform.parent == null)
                return _isActive;
            return transform.parent.owner.IsActive;
        }
        set
        {
            _isActive = value;
        }
    }

    /// <summary>
    /// Whether or not this object should be saved upon creating a template
    /// </summary>
    public bool IncludeWithSceneSerialization
    {
        get
        {
            if (!_includeWithSceneSerialization)
                return _includeWithSceneSerialization;

            if(transform.parent is not null)
            {
                return transform.parent.owner.IncludeWithSceneSerialization;
            }

            return _includeWithSceneSerialization;
        }
        set
        {
            _includeWithSceneSerialization = value;
        }
    }
    private bool _includeWithSceneSerialization = true;
    private bool _isActive = true;

    internal bool IsUIRoot => isUIRoot ??= components.Any(x => x is UICanvas);
    private bool? isUIRoot = null;

    /// <summary>
    /// If the object is somewhere in the hirarchy part of a <see cref="UICanvas"/> 
    /// this will return <see cref="RenderSpace.Screen"/>. otherwise, <see cref="RenderSpace.World"/>
    /// </summary>
    public RenderSpace RenderSpace
    {
        get
        {
            if (IsUIRoot)
                return RenderSpace.Screen;
            else if (transform.parent != null)
                return transform.parent.owner.RenderSpace;
            return RenderSpace.World;
        }
    }


    [IncludeWithSerialization]
    private List<ObjectComponent> components = new();
    private readonly List<Renderer> renderers = new();
    private readonly List<ActiveRenderer> activeRenderers = new();

    [ExcludeFromSerialization]
    internal MultipleParallelBehaviorHelper parallelHelper = new();

    [ExcludeFromSerialization]
    private readonly List<Action<WorldObject>> StartupBehaviors = new();
    [ExcludeFromSerialization]
    private readonly List<Action<WorldObject>> UpdateBehaviors = new();
    [ExcludeFromSerialization]
    private readonly List<Action<WorldObject, SpriteBatch>> DrawBehaviors = new();

    [IncludeWithSerialization]
    private long id;
    [IncludeWithSerialization]
    private Transform _transform;

    /// <summary>
    /// Creates a new ready to use object.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static WorldObject CreateNew(string name)
    {
        WorldObject newObj = new(name);
        newObj._SetTransform(newObj.AttachComponent<Transform>());
        return newObj;
    }

    /// <summary>
    /// A flag that can be used to identify this object. e.g. "Player", "Enemy", "Projectile"
    /// </summary>
    [IncludeWithSerialization]
    public string Flag { get; set; } = "";
    /// <summary>
    /// The amount of components the object has
    /// </summary>
    public int ComponentCount => components.Count;

    /// <summary>
    /// The chunk position this object is in
    /// </summary>
    [ExcludeFromSerialization]
    public ObjectChunkData ChunkPositionData { get; internal set; }
    [ExcludeFromSerialization]
    public bool IsDestroyed { get; internal set; }
    /// <summary>
    /// A shortcut to <see cref="Universe.CurrentWorld"/>
    /// </summary>
    public World world => transform.world;

    /// <summary>
    /// The index in the list of objects found in <see cref="World.objects"/>
    /// </summary>
    [ExcludeFromSerialization]
    internal int index = -1;

    [ExcludeFromSerialization]
    private int lastComponentCount = 0;

    [Show]
    [ExcludeFromSerialization]
    private TimeSpan updateTime;
    [Show]
    [ExcludeFromSerialization]
    private TimeSpan drawTime;


    public WorldObject() : this("New Object") { }
    public WorldObject(string name)
    {
        Name = name;
        id = WinterUtils.RandomLongID;
    }

    /// <summary>
    /// Adds a new startup behavior to this object. it is called when the object is started<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it.
    /// <br></br> NOTE: This is not saved.
    /// </summary>
    /// <param name="d"></param>
    public void AddStartupBehavior(Action<WorldObject> d) => StartupBehaviors.Add(d);
    /// <summary>
    /// Adds a new update behavior to this object. it is called when the object is updated<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it
    /// <br></br> NOTE: This is not saved.
    /// </summary>
    /// <param name="d"></param>
    public void AddUpdateBehavior(Action<WorldObject> d) => UpdateBehaviors.Add(d);
    /// <summary>
    /// Adds a new draw behavior to this object. it is called when the object is drawn<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it
    /// <br></br> NOTE: This is not saved.
    /// </summary>
    /// <param name="d"></param>
    public void AddDrawBehavior(Action<WorldObject, SpriteBatch> d) => DrawBehaviors.Add(d);

    /// <summary>
    /// Attaches a new component of type <typeparamref name="T"/> on this object. instantates a new instance of this component using <paramref name="args"/> as the constructor arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args">The arguments to use when creating the instance of the component</param>
    /// <returns>The newly created and added component</returns>
    /// <exception cref="Exception">Thrown when instance creation failed</exception>
    public T AttachComponent<T>(params object[] args) where T : ObjectComponent
    {
        var attr = typeof(T).GetCustomAttribute<ComponentLimitAttribute>();
        int maxComps = attr?.limit ?? -1;
        if (FetchComponents<T>().Length >= maxComps && maxComps != -1)
            throw new Exception($"Cant add more than {maxComps} of component type {typeof(T).Name} to a single object.");

        RequireComponentAttribute[] componentsRequired = typeof(T).GetCustomAttributes<RequireComponentAttribute>().ToArray();
        foreach (var required in componentsRequired)
        {
            if (!HasComponent(required.ComponentType))
            {
                if (required.AutoAdd)
                    AttachComponent(required.ComponentType, required.DefaultArguments);
                else
                    throw new RequiredComponentException($"component '{typeof(T).Name}' on object {Name} needs a component of type " +
                        $"{required.ComponentType.Name} to be added manually.");
            }
        }

        T comp = ActivatorExtra.CreateInstance<T>(args) ?? throw new Exception($"Couldnt create instance of type {typeof(T).Name}");
        AddComponent(comp);

        comp._owner = this;

        if (comp is Transform t)
        {
            if (_transform == null)
                _transform = t;
            else
                throw new InvalidOperationException("Cant add a second transform to a world object.");
        }

        if (transform is not null && (transform.world?.Initialized ?? false))
        {
            comp.CallAwake();
            comp.CallStart();

            transform.world.UpdateCameraIndexes();
        }

        return comp;
    }
    /// <summary>
    /// adds the component instance to this object
    /// </summary>
    /// <param name="component">the instance of the component to add</param>
    /// <returns><paramref name="component"/></returns>
    /// <exception cref="Exception"></exception>
    internal ObjectComponent AttachComponent(ObjectComponent component)
    {
        var attr = component.GetType().GetCustomAttribute<ComponentLimitAttribute>();
        int maxComps = attr?.limit ?? -1;
        if (FetchComponents(component.GetType()).Length >= maxComps && maxComps != -1)
            throw new Exception($"Cant add more than {maxComps} of component type {component.GetType().Name} to a single object.");

        component._owner = this;
        AddComponent(component);
        return component;
    }

    private void AddComponent(ObjectComponent comp)
    {
        isUIRoot = null;
        if (comp is Renderer renderer)
            renderers.Add(renderer);
        components.Add(comp);

        if (comp is ObjectBehavior e)
        {
            if (comp is ActiveRenderer r)
                activeRenderers.Add(r);

            if (e.IsParallel)
            {
                parallelHelper.Add(e);
                Universe.CurrentWorld?.RebuildParallelHelper();
            }
        }
    }

    /// <summary>
    /// Fetches the component of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>the found component, if none were found returns null</returns>
    public T? FetchComponent<T>() where T : class
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? c = components[i];
            if (c is T a)
                return a;
            if (c.GetType().IsAssignableTo(typeof(T)))
                return c as T;
        }

        return null;
    }
    /// <summary>
    /// Fetches the component of type <paramref name="type"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns>the found component, if none were found returns null</returns>
    public ObjectComponent? FetchComponent(Type type)
    {
        foreach (var c in components)
        {
            if (c.GetType() == type)
                return c;
            if (c.GetType().IsAssignableTo(type))
                return c;
        }
        return null;
    }
    /// <summary>
    /// Attempts to fetch a component of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <returns>True if one was found, <paramref name="component"/> will be the found component, if none were found returns False and <paramref name="component"/> will be null</returns>
    public bool TryFetchComponent<T>(out T component) where T : class => (component = FetchComponent<T>()!) is not null;
    /// <summary>
    /// Attempts to fetch a component of type <paramref name="type"/>
    /// </summary>
    /// <param name="component"></param>
    /// <returns>True if one was found, <paramref name="component"/> will be the found component, if none were found returns False and <paramref name="component"/> will be null</returns>
    public bool TryFetchComponent(Type type, out ObjectComponent? component) => (component = FetchComponent(type)) is not null;
    /// <summary>
    /// Fetches multiple components of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>An array of all found componnets</returns>
    public T[] FetchComponents<T>() where T : class
    {
        return components.Where(x => x is T).Cast<T>().ToArray();
    }
    /// <summary>
    /// Fetches an array of all components on this <see cref="WorldObject"/>
    /// </summary>
    /// <returns></returns>
    public ObjectComponent[] FetchComponents() => components.ToArray();
    /// <summary>
    /// Fetches multiple components of type <paramref name="type"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns>An array of all found components</returns>
    public ObjectComponent[] FetchComponents(Type type)
    {
        return components.Where(x => x.GetType() == type).ToArray();
    }
    /// <summary>
    /// Signals the <see cref="Worlds.Universe"/> to destroy this object
    /// </summary>
    public void Destroy()
    {
        Universe.CurrentWorld.Destroy(this);
    }
    /// <summary>
    /// Signals the <see cref="Worlds.Universe"/> to destroy <paramref name="obj"/>
    /// </summary>
    /// <param name="obj"></param>
    public void Destroy(WorldObject obj)
    {
        obj.Destroy();
    }
    /// <summary>
    /// Checks whether this object has a component of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Tue if it has, false it it doesnt</returns>
    public bool HasComponent<T>() where T : class => TryFetchComponent<T>(out _);
    /// <summary>
    /// Checks whether this object has a component of type <paramref name="type"/>
    /// </summary>
    /// <returns>Tue if it has, false it it doesnt</returns>
    public bool HasComponent(Type type) => TryFetchComponent(type, out _);
    /// <summary>
    /// Checks whether this object has at least one component of each type provided in <paramref name="componentTypes"/>
    /// </summary>
    /// <param name="componentTypes"></param>
    /// <returns>True if at least one component of each type in <paramref name="componentTypes"/> was found, otherwise false</returns>
    public bool HasComponents(params Type[] componentTypes)
    {
        foreach (var ct in componentTypes)
            if (TryFetchComponent(ct, out _)) return true;
        return false;
    }
    /// <summary>
    /// Removes the specified component
    /// </summary>
    /// <param name="component"></param>
    public void RemoveComponent(ObjectComponent component)
    {
        if (component is Transform)
            return; // cant remove transform component
        isUIRoot = null;
        component.CallClose();

        components.Remove(component);

        if (component is Renderer r)
            renderers.Remove(r);
        if (component is ActiveRenderer ar)
            activeRenderers.Remove(ar);

        RebuildParallelHelper();
    }

    private void RebuildParallelHelper()
    {
        parallelHelper = new();
        foreach (var c in components)
            if (c is ObjectBehavior b && b.IsParallel)
                parallelHelper.Add(b);

        Universe.CurrentWorld?.RebuildParallelHelper();
    }

    /// <summary>
    /// Removes all the components of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RemoveComponent<T>()
    {
        if (typeof(T) == typeof(Transform))
            return;
        isUIRoot = null;
        var col = components.Where(x => x is T).ToArray();
        for (int i = 0; i < col.Length; i++)
        {
            ObjectComponent? component = col[i];
            component.CallClose();
            RemoveComponent(component);
        }
    }
    /// <summary>
    /// returns "WorldObject: <see cref="Name"/>"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"WorldObject: '{Name}'";
    }
    /// <summary>
    /// Closes this object and all its components
    /// </summary>
    public void Close()
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            comp.CallClose();
            comp._owner = null;
        }

        components.Clear();
        _transform = null;
    }

    internal void WakeObject()
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            comp.CallAwake();
        }

        StartupBehaviors.Foreach(x => x.Invoke(this));
    }
    internal void StartObject()
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            comp.CallStart();
        }
    }
    internal void _SetTransform(Transform transform)
    {
        _transform = transform;
    }
    internal void OnWorldDestroy()
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            comp.CallClose();
            comp._owner = null;
        }
        components.Clear();
    }
    internal void Update()
    {
        if (!IsActive) return;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i] is not ObjectBehavior comp)
                continue;

            if (comp.Enabled)
            {
                if (comp.IsParallel)
                {
                    if (parallelHelper.Add(comp))
                        transform.world?.RebuildParallelHelper();
                }
                else
                    comp.CallUpdate();
            }
        }

        UpdateBehaviors.Foreach(x => x.Invoke(this));
        if (lastComponentCount != components.Count)
            Universe.RequestRender = true;
        lastComponentCount = components.Count;

        sw.Stop();
        updateTime = sw.Elapsed;
    }

    internal void Render(SpriteBatch batch)
    {
        if (!IsActive) return;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer.IsVisible && renderer.Enabled)
                renderer.Render(batch);
        }

        for (int i = 0; i < activeRenderers.Count; i++)
        {
            ActiveRenderer activeRenderer = activeRenderers[i];
            if (activeRenderer.IsVisible && activeRenderer.Enabled)
                activeRenderer.Render(batch);
        }

        DrawBehaviors.Foreach(x => x.Invoke(this, batch));
        sw.Stop();
        drawTime = sw.Elapsed;
    }

    /// <summary>
    /// Gets or adds the component of type <typeparamref name="T"/>. if it already exists it will return the existing component, otherwise it will create a new one
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public T FetchOrAttachComponent<T>(params object[] args) where T : ObjectComponent
    {
        if (HasComponent<T>())
            return FetchComponent<T>()!;
        return AttachComponent<T>(args);
    }

    /// <summary>
    /// Attatches a component of the given <paramref name="type"/> to this object
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public object AttachComponent(Type type)
    {
        MethodInfo info = GetType().GetMethod(nameof(AttachComponent), BindingFlags.Public | BindingFlags.Instance);
        MethodInfo generic = info.MakeGenericMethod(type);
        return generic.Invoke(this, null);
    }

    /// <summary>
    /// Attatches a component of the given <paramref name="type"/> to this object
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public object AttachComponent(Type type, object[] args)
    {
        Type[] types = [Array.Empty<object>().GetType()];

        MethodInfo info = GetType().GetMethod(nameof(AttachComponent), 1, BindingFlags.Public | BindingFlags.Instance, null, types, null);
        MethodInfo generic = info.MakeGenericMethod(type);
        return generic.Invoke(this, [args]);
    }

    /// <summary>
    /// Immediately destroys this object.
    /// <br></br> Can cause problems if called at the wrong time.
    /// </summary>
    public void DestroyImmediately()
    {
        transform.world.DestroyImmediately(this);
    }

    /// <summary>
    /// Gets or adds the component of the specified type <typeparamref name="T"/>. If it is created, uses <paramref name="args"/> as the constructor arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public T AttachOrFetchComponent<T>(params object[] args) where T : ObjectComponent
    {
        if (TryFetchComponent(out T component)) return component;
        return AttachComponent<T>(args);
    }

    internal void _setParent(WorldObject obj)
    {
        transform._parent = obj.transform;
    }

    internal void ValidateComponents()
    {
        foreach(var comp in components)
        {
            if(comp is Renderer r)
            {
                if(!renderers.Contains(r))
                    renderers.Add(r);
            }
            if(comp is ActiveRenderer ar)
            {
                if (!activeRenderers.Contains(ar))
                    activeRenderers.Add(ar);
            }
        }
    }
}