using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Encryption;
using WinterRose.FileManagement;
using WinterRose.Monogame.Worlds;
using WinterRose.Serialization;

namespace WinterRose.Monogame;

/// <summary>
/// A <see cref="Prefab"/> that contains a <see cref="WorldObject"/>.
/// </summary>
public sealed class WorldObjectPrefab : Prefab
{
    private readonly Lock threadLock = new();
    private Task? objectLoadTask;
    private static SerializerSettings serializerSettings = new()
    {
        IncludeType = true,
        CircleReferencesEnabled = true
    };

    /// <summary>
    /// For serializing
    /// </summary>
    private WorldObjectPrefab() : base("") { }
    public WorldObjectPrefab(string name, bool multithread = false) : base(name) 
    {
        Load(multithread);
    }

    public WorldObjectPrefab(string name, WorldObject obj, bool immediateAutoSave = true) : this(name)
    {
        LoadedObject = obj;

        if (immediateAutoSave)
            Save();
    }

    public WorldObject LoadedObject
    {
        get
        {
            Task t = objectLoadTask;
            if (t != null && t.Exception != null)
            {
                throw t.Exception;
            }

            if (obj == null)
            {
                if (t != null)
                {
                    if(!t.IsCompleted)
                        throw new Exception("Object was not finished loading!");
                }
                else
                    Load(false);
            }
                
            return obj!;
        }
        private set
        {
            obj = value;
        }
    }

    public bool HasLoaded => objectLoadTask is null 
                                            or not null and { IsCompletedSuccessfully: true };

    private WorldObject? obj;
    public static void Create(string name, WorldObject obj)
    {
        WorldObjectPrefab prefab = new(name);
        prefab.LoadedObject = obj;
        prefab.Save();
    }

    /// <summary>
    /// Loads the <see cref="WorldObject"/> from the File. This will overwrite the <see cref="LoadedObject"/> with the loaded object.
    /// <br></br> This will not add the object to the <see cref="World"/>.
    /// </summary>
    public override void Load()
    {
        Load(true);
    }

    public void Load(bool multithread)
    {
        if(multithread)
        {
            objectLoadTask = Task.Run(() =>
            {
                string content;
                while (true)
                {
                    try
                    {
                        content = File.ReadContent();
                        break;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                    {
                        System.Diagnostics.Debug.Write("Read failure: " + FileManager.PathFrom(File.File.FullName, "Content"));
                    }
                }
                var obj = SnowSerializer.Deserialize<WorldObject>(content, serializerSettings);
                this.obj = obj;
            });
        }
        else
        {
            string content = File.ReadContent();
            LoadedObject = SnowSerializer.Deserialize<WorldObject>(content, serializerSettings);
        }
    }

    /// <summary>
    /// Saves the <see cref="WorldObject"/> to the File.
    /// </summary>
    public override void Save()
    {
        string templateData = SnowSerializer.Serialize(LoadedObject, serializerSettings);
        File.WriteContent(templateData, true);
    }

    /// <summary>
    /// Unloads this <see cref="Prefab"/>. The instance of the <see cref="WorldObject"/> is not affected
    /// </summary>
    public override void Unload()
    {
        LoadedObject = null;
    }

    /// <summary>
    /// Loads the object prefab and returns the object. does not add it to the world.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static WorldObject Load(string prefabName, bool multithread)
    {
        var fab = new WorldObjectPrefab(prefabName, multithread);
        return fab.LoadedObject;
    }

    public static implicit operator WorldObject(WorldObjectPrefab prefab) => prefab.LoadedObject;
}

public class WorldObjectPrefabSerializer : CustomSerializer<WorldObjectPrefab>
{
    public override object Deserialize(string data, int depth)
    {
        var fab = new WorldObjectPrefab(data);
        return fab;
    }
    public override string Serialize(object obj, int depth)
    {
        WorldObjectPrefab fab = (WorldObjectPrefab)obj;
        fab.Save();
        return fab.Name;
    }
}
