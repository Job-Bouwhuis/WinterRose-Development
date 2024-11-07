using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WinterRose.Monogame.Attributes;
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
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }
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
    public bool IsActive { get; set; } = true;

    private readonly List<ObjectComponent> components = new();
    private readonly List<Action<WorldObject>> StartupBehaviors = new();
    private readonly List<Action<WorldObject>> UpdateBehaviors = new();
    private readonly List<Action<WorldObject, SpriteBatch>> DrawBehaviors = new();

    private string name;
    private long id;
    private Transform _transform;

    /// <summary>
    /// A flag that can be used to identify this object. e.g. "Player", "Enemy", "Projectile"
    /// </summary>
    [IncludeInTemplateCreation]
    public string Flag { get; set; } = "";
    public int ComponentCount => components.Count;

    /// <summary>
    /// The chunk position this object is in
    /// </summary>
    public ObjectChunkData ChunkPositionData { get; internal set; }
    public bool IsDestroyed { get; internal set; }

    /// <summary>
    /// The index in the list of objects found in <see cref="World.objects"/>
    /// </summary>
    internal int index = -1;

    private int lastComponentCount = 0;

    [Show]
    private TimeSpan updateTime;
    [Show]
    private TimeSpan drawTime;


    public WorldObject() : this("New Object") { }
    public WorldObject(string name)
    {
        this.name = name;
        id = WinterUtils.RandomLongID;
    }

    /// <summary>
    /// Adds a new startup behavior to this object. it is called when the object is started<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it
    /// </summary>
    /// <param name="d"></param>
    public void AddStartupBehavior(Action<WorldObject> d) => StartupBehaviors.Add(d);
    /// <summary>
    /// Adds a new update behavior to this object. it is called when the object is updated<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it
    /// </summary>
    /// <param name="d"></param>
    public void AddUpdateBehavior(Action<WorldObject> d) => UpdateBehaviors.Add(d);
    /// <summary>
    /// Adds a new draw behavior to this object. it is called when the object is drawn<br></br>
    /// This is different than a component on the object, this is just a function that has no relation to this object other than being called from it
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
                    throw new RequiredComponentException($"component {typeof(T).Name} on object {Name} needs a component of type {required.ComponentType.Name} to be added manually.");
            }
        }

        T comp = ActivatorExtra.CreateInstance<T>(args) ?? throw new Exception($"Couldnt create instance of type {typeof(T).Name}");
        components.Add(comp);

        comp._owner = this;

        if (comp is Transform t)
        {
            _transform = t;
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
    public ObjectComponent AttachComponent(ObjectComponent component)
    {
        var attr = component.GetType().GetCustomAttribute<ComponentLimitAttribute>();
        int maxComps = attr?.limit ?? -1;
        if (FetchComponents(component.GetType()).Length >= maxComps && maxComps != -1)
            throw new Exception($"Cant add more than {maxComps} of component type {component.GetType().Name} to a single object.");

        component._owner = this;
        components.Add(component);
        return component;
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
        MethodInfo tryFetch = GetType().GetMethod(nameof(TryFetchComponent), BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < componentTypes.Length; i++)
        {
            Type? type = componentTypes[i];
            MethodInfo generic = tryFetch.MakeGenericMethod(type);

            var res = generic.Invoke(this, null);
            if (res is null)
                return false;
        }
        return true;
    }
    /// <summary>
    /// Removes the specified component
    /// </summary>
    /// <param name="component"></param>
    public void RemoveComponent(ObjectComponent component)
    {
        if (component is Transform)
            return;
        component.CallClose();
        components.Remove(component);
    }
    /// <summary>
    /// Removes all the components of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RemoveComponent<T>()
    {
        if (typeof(T) == typeof(Transform))
            return;
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
        }

        components.Clear();
        _transform = null;
    }


    internal void PostTemplateLoad()
    {
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            comp._owner = this;
        }
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
        List<PhysicsObject> physicsobjs = [];

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            if (IsActive && comp.Enabled)
            {
                if (comp is ObjectBehavior behavior)
                    behavior.CallUpdate();
                if (comp is PhysicsObject physicsObject)
                    physicsObject.UpdatePhysicsSubSteps(Time.SinceLastFrame, 1);
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
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < components.Count; i++)
        {
            ObjectComponent? comp = components[i];
            if (comp is Renderer renderer && renderer.IsVisible && comp.Enabled)
                renderer.Render(batch);
            else if (comp is ActiveRenderer activeRenderer && activeRenderer.IsVisible && comp.Enabled)
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
}