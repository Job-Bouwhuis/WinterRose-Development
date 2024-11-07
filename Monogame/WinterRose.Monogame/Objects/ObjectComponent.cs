using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Defines that te derived class is a component. provides callback methods for when the component is created, enabled, en being destroyed
    /// </summary>
    public abstract class ObjectComponent
    {
        /// <summary>
        /// The time it took to run the Awake method from start to finish
        /// </summary>
        public TimeSpan AwakeTime => awakeTime;
        /// <summary>
        /// The time it took to run the Start method from start to finish
        /// </summary>
        public TimeSpan StartTime => startTime;
        /// <summary>
        /// The time it took to run the Close method from start to finish
        /// </summary>
        public TimeSpan CloseTime => closeTime;
        /// <summary>
        /// The <see cref="WorldObject"/> on which this component is attached
        /// </summary>
        [Hidden]
        public WorldObject owner => _owner;
        /// <summary>
        /// The <see cref="Transform"/> of <see cref="owner"/>
        /// </summary>
        [Hidden]
        public Transform transform => owner.transform;
        /// <summary>
        /// Whether this component is enabled or not
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The chunk in which <see cref="owner"/> is located (based on the origin position of the object)
        /// </summary>
        [Hidden]
        public WorldChunk Chunk
        {
            get
            {
                WorldChunk? chunk = null;
                if(owner.ChunkPositionData is not null)
                    chunk = owner.ChunkPositionData.ChunkContainingObjectOrigin;
                if (chunk is null)
                {
                    world.WorldChunkGrid.GenerateChunks((Vector2I)transform.position);
                    chunk = world.WorldChunkGrid.GetChunkAt((Vector2I)transform.position);
                }
                return chunk;
            }
        }

        /// <summary>
        /// Gets the value from <see cref="Universe.CurrentWorld"/>. this exists to make life easier
        /// </summary>
        [Hidden]
        public World world => Universe.CurrentWorld;

        private MethodInfo? awakeMethod;
        private MethodInfo? startMethod;
        private MethodInfo? closeMethod;
        [Show]
        private TimeSpan awakeTime;
        [Show]
        private TimeSpan startTime;
        [Show]
        private TimeSpan closeTime;
        internal WorldObject _owner;

        internal bool initialized = false;

        public ObjectComponent()
        {
            Initialize();
        }

        internal virtual void Initialize()
        {
            OverrideDefaultMethodNamesAttribute? attr = GetType().GetCustomAttribute<OverrideDefaultMethodNamesAttribute>();
            Type t = GetType();

            if (attr != null)
            {
                awakeMethod = t.GetMethod(attr.Awake, MonoUtils.InstanceMemberFindingFlags);
                startMethod = t.GetMethod(attr.Start, MonoUtils.InstanceMemberFindingFlags);
                closeMethod = t.GetMethod(attr.Close, MonoUtils.InstanceMemberFindingFlags);
            }
            else
            {
                awakeMethod = t.GetMethod("Awake", MonoUtils.InstanceMemberFindingFlags);
                startMethod = t.GetMethod("Start", MonoUtils.InstanceMemberFindingFlags);
                closeMethod = t.GetMethod("Close", MonoUtils.InstanceMemberFindingFlags);
            }
            initialized = true;
        }

        internal void CallAwake()
        {
            if (!initialized)
                Initialize();
            var sw = Stopwatch.StartNew();
            awakeMethod?.Invoke(this, null);
            sw.Stop();
            awakeTime = sw.Elapsed;
        }
        internal void CallStart()
        {
            if (initialized)
                Initialize();
            var s = Stopwatch.StartNew();
            startMethod?.Invoke(this, null);
            s.Stop();
            startTime = s.Elapsed;
        }
        internal void CallClose()
        {
            if (initialized)
                Initialize();
            var sw = Stopwatch.StartNew();
            closeMethod?.Invoke(this, null);
            sw.Stop();
            closeTime = sw.Elapsed;
        }

        /// <summary>
        /// Tries to fetch a component of the given type <typeparamref name="T"/> from <see cref="owner"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The first component that was found, if none were found, returns null</returns>
        public T? FetchComponent<T>() where T : class => owner.FetchComponent<T>();
        /// <summary>
        /// Tries to fetch multiple of the given type <typeparamref name="T"/> components from <see cref="owner"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] FetchComponents<T>() where T : class => owner.FetchComponents<T>();
        /// <summary>
        /// Tries to fetch a component of the given type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns>True if a component was found, <paramref name="component"/> will be this found componnet.<br></br>
        /// If no component was found, returns false, and <paramref name="component"/> will be null</returns>
        public bool TryFetchComponent<T>(out T component) where T : class => owner.TryFetchComponent(out component);
        /// <summary>
        /// Checks whether there is a component of type <typeparamref name="T"/> on <see cref="owner"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if a component of type <typeparamref name="T"/> was found, otherwise false </returns>
        public bool HasComponent<T>() where T : class => owner.HasComponent<T>();

        /// <summary>
        /// Attaches the given component to <see cref="owner"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns>The attached component</returns>
        public T AttachComponent<T>(params object[] args) where T : ObjectComponent
        {
            return owner.AttachComponent<T>(args);
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
            return AttachOrFetchComponent<T>(args);
        }

        /// <summary>
        /// Gets or adds a component of type <typeparamref name="T"/> to <see cref="owner"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T FetchOrAttachComponent<T>(params object[] args) where T : ObjectComponent
        {
            return owner.FetchOrAttachComponent<T>(args);
        }

        /// <summary>
        /// Destroys the <see cref="owner"/> of this component
        /// </summary>
        public void Destroy()
        {
            owner.Destroy();
        }

        public void Destroy(WorldObject @object)
        {
            owner.Destroy(@object);
        }

        internal virtual ObjectComponent Clone(WorldObject newOwner)
        {
            ObjectComponent shallowClone = (ObjectComponent)MemberwiseClone();
            shallowClone._owner = newOwner;
            ResetClone(shallowClone);
            return shallowClone;
        }

        public virtual void ResetClone(in ObjectComponent newComponent)
        {
            newComponent.awakeMethod = null;
            newComponent.closeMethod = null;
            newComponent.startMethod = null;
            newComponent.initialized = false;
        }
    }
}
