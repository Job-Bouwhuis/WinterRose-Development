﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Monogame.EditorMode;
using WinterRose.Monogame.UI;
using WinterRose.Monogame.TextRendering;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Formatting;
using WinterRose.WinterForgeSerializing.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using WinterRose.WinterThornScripting;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A world where objects can be placed, updated, and rendered using various <see cref="ObjectComponent"/> or <see cref="ObjectBehavior"/>
/// </summary>
[IncludePrivateFields]
[SerializeAs<World>]
public sealed class World : IEnumerable<WorldObject>
{
    /// <summary>
    /// The name of the world
    /// </summary>
    [IncludeWithSerialization]
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

    [ExcludeFromSerialization]
    private MultipleParallelBehaviorHelper parallelHelper = new();

    /// <summary>
    /// The grid of chunks for this world
    /// </summary>
    public WorldGrid WorldChunkGrid { get; set; }

    // the list of all objects in this world
    private List<WorldObject> objects = [];

    [ExcludeFromSerialization]
    private ConcurrentBag<NewObj> nextToSpawn = [];
    /// <summary>
    /// All objects in this world
    /// </summary>
    public List<WorldObject> Objects
    {
        get
        {
            return objects;
        }
    }
    /// <summary>
    /// a list of all objects to be destroyed at the end of the update
    /// </summary>
    private readonly List<WorldObject> ToDestroy = [];

    // the total time it took to update and draw the world
    [ExcludeFromSerialization]
    private TimeSpan totalDrawTime = TimeSpan.Zero;
    [ExcludeFromSerialization]
    private TimeSpan totalUpdateTime = TimeSpan.Zero;

    [ExcludeFromSerialization]
    private float time;
    [ExcludeFromSerialization]
    private int updatesPerSecond;
    [ExcludeFromSerialization]
    private int updates;
    [ExcludeFromSerialization]
    private int drawsPerSecond;
    [ExcludeFromSerialization]
    private int draws;

    [ExcludeFromSerialization]
    bool runOnce = true;

    [ExcludeFromSerialization]
    private Vector2I lastScreenBounds;
    [ExcludeFromSerialization]
    RenderTarget2D renderTarget;
    [ExcludeFromSerialization]
    internal bool rebuildParallelHelper;

    /// <summary>
    /// Creates a new empty world
    /// </summary>
    public World() : this("New Empty World")
    {
    }
    /// <summary>
    /// Creates a new empty world with the given name
    /// </summary>
    /// <param name="name"></param>
    public World(string name)
    {
        Name = name;
        WorldChunkGrid = new WorldGrid(this);
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
    /// Creates a new world from the given template type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static World FromTemplate<T>(params object[] args) where T : WorldTemplate
    {
        T template = ActivatorExtra.CreateInstance<T>(args);
        var world = new World(template.Name);
        template.Build(world);
        return world;
    }
    /// <summary>
    /// Creates a new world from the given template type
    /// </summary>
    /// <returns></returns>
    public static World FromTemplate(Type t, params object[] args)
    {
        if (!t.IsAssignableTo(typeof(WorldTemplate)))
            throw new InvalidOperationException($"Given type '{t.FullName}' does not inherit from {nameof(WorldTemplate)}");
        WorldTemplate template = (WorldTemplate)ActivatorExtra.CreateInstance(t, args);
        var world = new World(template.Name);
        template.Build(world);
        return world;
    }

    public static World FromTemplateFile(string templateFile, WinterForgeProgressTracker? progressTracker = null)
    {
        using FileStream file = File.Open("Content/Worlds/" + templateFile + ".world", FileMode.Open, FileAccess.Read);
        using GZipStream decompressed = new GZipStream(file, CompressionMode.Decompress);
        return WinterForge.DeserializeFromStream<World>(decompressed, progressTracker);

    }

    /// <summary>
    /// Saves all the objects to a file with the given
    /// name to reference it later in <see cref=FromTemplate(string)"/>
    /// </summary>
    public void SaveTemplate()
    { 
        HandleNewToAdd();

        World savingWorld = new World(Name);
        foreach (var obj in objects)
        {
            if (obj.IncludeWithSceneSerialization)
                savingWorld.objects.Add(obj);
        }

        if (!Directory.Exists("Content/Worlds"))
            Directory.CreateDirectory("Content/Worlds");

        using FileStream file = File.Open("Content/Worlds/" + Name + ".world", FileMode.Create, FileAccess.Write);
        using GZipStream compressed = new GZipStream(file, CompressionLevel.SmallestSize);
        WinterForge.SerializeToStream(savingWorld, compressed);
    }
    /// <summary>
    /// Calls the <c>Awake</c> and <c>Start</c> methods on all components on all objects that have these methods implemented
    /// </summary>
    public void WakeWorld()
    {
        HandleNewToAdd();

        foreach (var obj in objects)
        {
            obj.ValidateComponents();
            obj.WakeObject();
        }
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
        return Duplicate(prefab.LoadedObject, prefab.LoadedObject.Name);
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
        for (int i = 0; i < objects.Count; i++)
        {
            WorldObject? obj = objects[i];
            if (obj is null || obj.IsDestroyed)
                continue;
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
        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
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

        HandleNewToAdd();

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
                if (obj is null || obj.IsDestroyed)
                    continue;
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
            DoDestroyObject(obj);
        }
        if (ToDestroy.Count > 0)
        {
            UpdateCameraIndexes();
            rebuildParallelHelper = true;
        }

        ToDestroy.Clear();

        sw.Stop();
        totalUpdateTime = sw.Elapsed;

        time += Time.deltaTime;
        if (time > 1)
        {
            updatesPerSecond = updates;
            updates = 0;
            drawsPerSecond = draws;
            draws = 0;
            time = 0;
        }
    }

    private void HandleNewToAdd()
    {
        List<NewObj> unreadObjects = [];
        while (nextToSpawn.TryTake(out var newObj))
        {
            WorldObject toAdd;

            if (newObj is NewObjPrefab fab)
            {
                if (!fab.fab.HasLoaded)
                {
                    unreadObjects.Add(fab);
                    continue;
                }
                toAdd = fab.fab.LoadedObject;
            }
            else
                toAdd = newObj.obj;

            objects.Add(toAdd);
            if (newObj.configure is not null)
                newObj.configure(toAdd);
            if (Initialized)
            {
                toAdd.WakeObject();
                toAdd.StartObject();
            }
        }

        unreadObjects.Foreach(nextToSpawn.Add);
    }

    private void DoDestroyObject(WorldObject obj)
    {
        if (obj.transform is not null)
        {
            foreach (var kid in obj.transform) // destroy object children recursively
                DoDestroyObject(kid.owner);
        }

        obj.Close();
        obj.IsDestroyed = true;

        objects.Remove(obj);

        if (Initialized)
            WorldChunkGrid.Remove(obj);
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
            if (obj is null || obj.IsDestroyed)
                continue;
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
    public (RenderTarget2D world, RenderTarget2D? UI) Render(SpriteBatch batch, int cameraIndex)
    {
        draws++;
        var sw = Stopwatch.StartNew();
        try
        {
            var cameras = FindComponents<Camera>();
            if (cameraIndex is -1 || cameras.Length is 0)
            {
                RenderTarget2D targetWorld = GetRenderTarget();
                MonoUtils.Graphics.SetRenderTarget(targetWorld);

                Universe.StartNoCameraSpritebatch(batch);
                var objs = RenderWorld(batch);

                foreach (var obj in objs)
                    obj.Render(batch);
                batch.End();

                MonoUtils.Graphics.SetRenderTarget(null);
                return (targetWorld, null);
            }
            else
                renderTarget = null;

            Camera cam = cameras[cameraIndex];

            var (camView, UIObjects) = cam.GetCameraView(RenderWorld);

            batch.Begin();
            batch.Draw(camView, new Vector2(0f, 0f), null,
                Color.White, 0, new(), 1, SpriteEffects.None, 0f);

            batch.End();

            RenderTarget2D? UItarget = null;

            UItarget = GetRenderTarget();
            MonoUtils.Graphics.SetRenderTarget(UItarget);
            MonoUtils.Graphics.Clear(new Color(0, 0, 0, 0));
            batch.Begin(SpriteSortMode.FrontToBack);

            foreach (var obj in UIObjects)
                obj.Render(batch);

            Debug.RenderScreenSpaceDrawRequests(batch);

            batch.End();

            MonoUtils.Graphics.SetRenderTarget(null);

            return (camView, UItarget);
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
            return (null, null);
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
        result.Flag = obj.Flag;
        result.IncludeWithSceneSerialization = obj.IncludeWithSceneSerialization;

        foreach (var comp in obj.FetchComponents())
        {
            if (comp is Transform)
                continue;
            ObjectComponent newComp = comp.Clone(result);
            result.AttachComponent(newComp);
        }

        if (Initialized)
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
            renderTarget = new(MonoUtils.Graphics,
                               MonoUtils.WindowResolution.X,
                               MonoUtils.WindowResolution.Y,
                               false,
                               SurfaceFormat.Color,
                               DepthFormat.None);
        }
        return renderTarget;
    }
    private List<WorldObject> RenderWorld(SpriteBatch batch)
    {
        List<WorldObject> UIObjects = [];
        for (int i = 0; i < objects.Count; i++)
        {
            WorldObject? obj = objects[i];
            if (obj is null || obj.IsDestroyed)
                continue;
            if (UIObjects.Contains(obj))
                continue;
            if (obj.IsUIRoot)
            {
                CommitKids(obj, UIObjects);
                continue;
            }
            obj.Render(batch);
        }

        Primitives2D.CommitDraw(batch);
        Debug.RenderWorldSpaceDrawRequests(batch);
        return UIObjects;
    }

    private void CommitKids(WorldObject parent, List<WorldObject> list)
    {
        if (parent == null)
        {
            return;
        }
        list.Add(parent);
        foreach (var obj in parent.transform)
            CommitKids(obj.owner, list);
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
        DoDestroyObject(worldObject);
    }

    public WorldObject InstantiateExact(WorldObject obj)
    {
        objects.Add(obj);

        foreach (var o in obj.transform)
        {
            InstantiateExact(o.owner);
        }

        if (Initialized)
        {
            obj.WakeObject();
            obj.StartObject();
        }
        return obj;
    }

    internal void RebuildParallelHelper() => rebuildParallelHelper = true;
    /// <summary>
    /// Searches for an object with the given <see cref="WorldObject.Flag"/>
    /// </summary>
    /// <param name="targetFlag"></param>
    /// <returns></returns>
    public WorldObject? FindObjectWithFlag(string targetFlag)
    {
        foreach (var obj in objects)
            if (obj.Flag == targetFlag)
                return obj;
        return null;
    }

    /// <summary>
    /// Searches for all objects with the given <see cref="WorldObject.Flag"/>
    /// </summary>
    /// <param name="targetFlag"></param>
    /// <returns></returns>
    public List<WorldObject> FindObjectsWithFlag(string targetFlag)
    {
        List<WorldObject> result = [];
        foreach (var obj in objects)
            if (obj.Flag == targetFlag)
                result.Add(obj);
        return result;
    }

    public void Instantiate(WorldObject obj, Action<WorldObject> configureObj = null, bool forceDelayed = false)
    {
        obj.IncludeWithSceneSerialization = false;

        int id = System.Threading.Thread.GetCurrentProcessorId();
        if (id == Application.Current.ApplicationMainThreadID && !forceDelayed)
        {
            WorldObject o = Duplicate(obj, obj.Name);
            if (configureObj is not null)
                configureObj(o);
            if (Initialized)
            {
                o.WakeObject();
                o.StartObject();
            }
        }
        else
        {
            nextToSpawn.Add(new(obj, configureObj));
        }

        foreach (var o in obj.transform)
        {
            InstantiateExact(o.owner);
        }
    }

    public void SchedulePrefabSpawn(WorldObjectPrefab fab, Action<WorldObject> configureObj = null)
    {
        nextToSpawn.Add(new NewObjPrefab(fab, configureObj));
    }

    private record NewObj(WorldObject obj, Action<WorldObject>? configure);
    private record NewObjPrefab(WorldObjectPrefab fab, Action<WorldObject>? configure) : NewObj(null, configure);
}
