using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinterRose.Monogame.EditorMode;
using WinterRose.Monogame.UI;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A world where objects can be placed, updated, and rendered using various <see cref="ObjectComponent"/> or <see cref="ObjectBehavior"/>
/// </summary>
public sealed class World : IEnumerable<WorldObject>
{
    /// <summary>
    /// The name of the world
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The amount of times per second the world is updated
    /// </summary>
    public int UpdatesPerSecond => updatesPerSecond;
    /// <summary>
    /// The amount of times per second the world is drawn
    /// </summary>
    public int DrawsPerSecond => drawsPerSecond;
    /// <summary>
    /// The total time it took to update the world
    /// </summary>
    public TimeSpan TotalUpdateTime => totalUpdateTime;
    /// <summary>
    /// The total time it took to draw the world
    /// </summary>
    public TimeSpan TotalDrawTime => totalDrawTime;
    /// <summary>
    /// The amount of objects in this world
    /// </summary>
    public int ObjectCount => objects.Count;
    /// <summary>
    /// Gets whether the world has been initialized
    /// </summary>
    public bool Initialized { get; internal set; }
    public int ComponentCount => objects.Sum(x => x.ComponentCount);

    private MultipleParallelBehaviorHelper parallelHelper = new();

    /// <summary>
    /// The grid of chunks for this world
    /// </summary>
    public WorldGrid WorldChunkGrid { get; set; }

    // the list of all objects in this world
    private List<WorldObject> objects = [];
    /// <summary>
    /// All objects in this world
    /// </summary>
    public List<WorldObject> Objects => [.. objects];
    /// <summary>
    /// a list of all objects to be destroyed at the end of the update
    /// </summary>
    private readonly List<WorldObject> ToDestroy = [];

    // the total time it took to update and draw the world
    private TimeSpan totalDrawTime = TimeSpan.Zero;
    private TimeSpan totalUpdateTime = TimeSpan.Zero;

    private float time;
    private int updatesPerSecond;
    private int updates;
    private int drawsPerSecond;
    private int draws;

    bool runOnce = true;

    private Vector2I lastScreenBounds;
    RenderTarget2D renderTarget;
    internal bool rebuildParallelHelper;

    /// <summary>
    /// Creates a new empty world
    /// </summary>
    public World() : this("New Empty World")
    {
        Initialized = true;
    }
    /// <summary>
    /// Creates a new empty world with the given name
    /// </summary>
    /// <param name="name"></param>
    public World(string name)
    {
        Name = name;
        Initialized = true;
        WorldChunkGrid = new WorldGrid(this);
    }
    /// <summary>
    /// Creates a new world from a template file
    /// </summary>
    /// <param name="name"></param>
    /// <param name="templatePath"></param>
    /// <param name="multiThread"></param>
    /// <param name="callback"></param>
    public World(string name, FilePath templatePath, bool multiThread = false, Action<string>? callback = null) : this(name)
    {
        Initialized = false;
        if (!templatePath.HasExtention(".world"))
            templatePath /= ".world";

        WorldTemplateLoader loader = new(this);
        if (callback is not null)
            loader.Callback += callback;

        if (multiThread)
            loader.LoadTemplateultiThread(templatePath);
        else
            loader.LoadTemplate(templatePath);

        foreach (var obj in objects)
            obj.PostTemplateLoad();

        Initialized = true;
    }
    /// <summary>
    /// Creates a new world using the template type. Must be a subclass of <see cref="WorldTemplate"/>
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentException"></exception>
    public World(Type type) : this(type.Name)
    {
        if (!type.IsSubclassOf(typeof(WorldTemplate)))
            throw new ArgumentException("Type must be a subclass of WorldTemplate", nameof(type));

        var template = Activator.CreateInstance(type) as WorldTemplate;

        template.Build(this);
    }
    /// <summary>
    /// Creates a new world from a template file asynchronously
    /// </summary>
    /// <param name="name">the name of the world to be created</param>
    /// <param name="templatePath">the path to the template file</param>
    /// <param name="multiThreaded">whether to parse multiple objects at the same time</param>
    /// <param name="callback">a callback for progress</param>
    /// <returns></returns>
    public static async Task<World> FromTemplateAsync(string name, string templatePath, bool multiThreaded = false, Action<string>? callback = null)
    {
        return await Task.Run(() =>
        {
            return new World(name, templatePath, multiThreaded, callback);
        });
    }
    /// <summary>
    /// Creates a new world from the given template type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static World FromTemplate<T>() where T : WorldTemplate
    {
        T template = Activator.CreateInstance<T>();
        var world = new World(template.Name);
        template.Build(world);
        return world;
    }
    /// <summary>
    /// Calls the <c>Awake</c> and <c>Start</c> methods on all components on all objects that have these methods implemented
    /// </summary>
    public void WakeWorld()
    {
        foreach (var obj in objects)
            obj.WakeObject();
        foreach (var obj in objects)
            obj.StartObject();
    }
    /// <summary>
    /// Creates a new <see cref="WorldObject"/> within this world
    /// </summary>
    /// <param name="name">The name of the object</param>
    /// <returns>The created object</returns>
    public WorldObject CreateObject(string name)
    {
        WorldObject obj = new WorldObject(name);
        obj._SetTransform(obj.AttachComponent<Transform>());
        objects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Creates the prefab object into the world.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public WorldObject CreateObject(WorldObjectPrefab prefab)
    {
        return prefab.LoadIn(this);
    }

    /// <summary>
    /// Creates a new <see cref="WorldObject"/> within this world with the component <typeparamref name="T"/> attached. <paramref name="args"/> is passed to the constructor of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of component to create</typeparam>
    /// <param name="name">The name of the object to be created</param>
    /// <param name="args">The arguments to pass to the constructor of <typeparamref name="T"/></param>
    /// <returns>The created component</returns>
    public T CreateObject<T>(string name, params object[] args) where T : ObjectComponent
    {
        var obj = CreateObject(name);
        return obj.AttachComponent<T>(args);
    }

    /// <summary>
    /// Returns the amount of cameras in the world
    /// </summary>
    /// <returns></returns>
    internal int UpdateCameraIndexes()
    {
        int index = 0;
        foreach (var obj in objects)
        {
            if (obj.TryFetchComponent<Camera>(out var cam))
            {
                cam.CameraIndex = index;
                index++;
            }
        }
        return index;
    }

    public Camera GetCamera(int cameraIndex)
    {
        var cameras = FindComponents<Camera>();
        if (cameraIndex < 0 || cameraIndex > cameras.Length)
            return null;
        return cameras[cameraIndex];
    }

    /// <summary>
    /// Returns the index of the given camera in the world
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    internal int GetCameraIndex(Camera cam)
    {
        UpdateCameraIndexes();
        return cam.CameraIndex;
    }

    /// <summary>
    /// Calls <c>Update</c> on all <see cref="ObjectBehavior"/> of all objects in this world if they are active,
    /// </summary>
    public void Update()
    {
        if (runOnce)
        {
            WorldChunkGrid.UpdateObjectChunkPositions(objects);
            UpdateCameraIndexes();
            runOnce = false;
        }

        if (rebuildParallelHelper)
        {
            parallelHelper.Dispose();
            parallelHelper = new();
        }

        updates++;
        var sw = Stopwatch.StartNew();

        if (!Editor.Opened)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                WorldObject? obj = objects[i];

                if (obj.IsActive)
                    obj.Update();
                if (rebuildParallelHelper)
                {
                    parallelHelper += obj.parallelHelper;
                }
            }

            parallelHelper.Execute();

            if (rebuildParallelHelper)
                rebuildParallelHelper = false;
        }

        WorldChunkGrid.UpdateObjectChunkPositions(objects);

        foreach (var obj in ToDestroy)
        {
            obj.Close();
            obj.IsDestroyed = true;

            objects.Remove(obj);


            WorldChunkGrid.Remove(obj);
        }
        if(ToDestroy.Count > 0)
        {
            UpdateCameraIndexes();
            rebuildParallelHelper = true;
        }

        ToDestroy.Clear();

        sw.Stop();
        totalUpdateTime = sw.Elapsed;

        time += Time.SinceLastFrame;
        if (time > 1)
        {
            updatesPerSecond = updates;
            updates = 0;
            drawsPerSecond = draws;
            draws = 0;
            time = 0;
        }
    }
    /// <summary>
    /// Finds all components of the given type <typeparamref name="T"/> in the world
    /// </summary>
    /// <typeparam name="T">The type of component to seach for</typeparam>
    /// <returns>An array of found components. array will be empty if none are found</returns>
    public T[] FindComponents<T>() where T : class
    {
        var list = new List<T>();
        for (int i = 0; i < objects.Count; i++)
        {
            WorldObject? obj = objects[i];
            if (obj.TryFetchComponent<T>(out var res))
                list.Add(res!);
        }

        return list.ToArray();
    }
    /// <summary>
    /// Finds the object with the given name in the world
    /// </summary>
    /// <param name="name">The name of the object to search for</param>
    /// <returns>The found object, if no object is found returns null</returns>
    public WorldObject? this[string name]
    {
        get
        {
            foreach (var obj in objects)
            {
                if (obj.Name == name)
                    return obj;
            }
            return null;
        }
    }
    /// <summary>
    /// Gets the object at the specified index. if the index is out of range returns null
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public WorldObject? this[int index]
    {
        get
        {
            var objs = Objects;
            if (objs.Count > index)
                return objs[index];
            return null;
        }
    }
    /// <summary>
    /// Gets the enumerator for all objects in this world
    /// </summary>
    /// <returns></returns>
    public IEnumerator<WorldObject> GetEnumerator()
    {
        return objects.GetEnumerator();
    }
    /// <summary>
    /// Renders the world to a <see cref="RenderTarget2D"/> and returns it
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="cameraIndex"></param>
    /// <returns></returns>
    public RenderTarget2D Render(SpriteBatch batch, int cameraIndex)
    {
        draws++;
        var sw = Stopwatch.StartNew();
        try
        {
            var cameras = FindComponents<Camera>();
            if (cameraIndex is -1 || cameras.Length is 0)
            {
                RenderTarget2D target = GetRenderTarget();
                MonoUtils.Graphics.SetRenderTarget(target);

                Universe.StartNoCameraSpritebatch(batch);
                RenderWorld(batch);
                batch.End();

                MonoUtils.Graphics.SetRenderTarget(null);
                return target;
            }
            else
                renderTarget = null;

            Camera cam = cameras[cameraIndex];

            RenderTarget2D camView = cam.GetCameraView(RenderWorld);

            batch.Begin();
            batch.Draw(camView, new Vector2(0f, 0f), Color.White);
            batch.End();
            return camView;
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            try
            {
                batch.End();
                MonoUtils.Graphics.SetRenderTarget(null);
            }
            catch { } // if the batch is already ended this will throw an exception, so we catch it here
            return null;
        }
        finally
        {
            sw.Stop();
            totalDrawTime = sw.Elapsed;
        }
    }
    /// <summary>
    /// Destroys the world by calling Destroy on all objects in the world
    /// </summary>
    public void Destroy()
    {
        foreach (var obj in objects)
            obj.OnWorldDestroy();
        objects.Clear();
    }
    /// <summary>
    /// Adds the given <paramref name="worldObject"/> to the list of objects to be destroyed at the end of the update
    /// </summary>
    /// <param name="worldObject"></param>
    public void Destroy(WorldObject worldObject)
    {
        ToDestroy.Add(worldObject);
    }
    /// <summary>
    /// Creates a new object with the same components and position as the given object
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="newName"></param>
    public WorldObject Duplicate(WorldObject obj, string newName)
    {
        WorldObject result = CreateObject(newName);
        result.transform.position = obj.transform.position;
        result.transform.rotation = obj.transform.rotation;
        result.transform.scale = obj.transform.scale;

        foreach (var comp in obj.FetchComponents())
        {
            if (comp is Transform)
                continue;
            ObjectComponent newComp = comp.Clone(result);
            result.AttachComponent(newComp);
        }

        if(Initialized)
        {
            result.WakeObject();
            result.StartObject();
        }
        return result;
    }

    private RenderTarget2D GetRenderTarget()
    {
        if (renderTarget is null || lastScreenBounds != MonoUtils.WindowResolution)
        {
            lastScreenBounds = MonoUtils.WindowResolution;
            renderTarget = new(MonoUtils.Graphics, MonoUtils.WindowResolution.X, MonoUtils.WindowResolution.Y);
        }
        return renderTarget;
    }
    private void RenderWorld(SpriteBatch batch)
    {
        foreach (var obj in objects)
        {
            obj.Render(batch);
        }

        Primitives2D.CommitDraw(batch);
        Debug.RenderDrawRequests(batch);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Removes the object the moment the function is called. Does not call 'Close' on the object
    /// <br></br>Can cause problems if called at the wrong time.
    /// </summary>
    /// <param name="worldObject"></param>
    public void DestroyImmediately(WorldObject worldObject)
    {
        objects.Remove(worldObject);
    }

    internal void InstantiateExact(WorldObject obj)
    {
        objects.Add(obj);
    }

    internal void RebuildParallelHelper() => rebuildParallelHelper = true;
}
