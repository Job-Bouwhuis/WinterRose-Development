using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Encryption;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame;

/// <summary>
/// A <see cref="Prefab"/> that contains a <see cref="WorldObject"/>.
/// </summary>
public sealed class WorldObjectPrefab : Prefab
{
    public WorldObjectPrefab(string name) : base(name) { }

    public WorldObjectPrefab(string name, WorldObject obj, bool immediateAutoSave = true) : this(name)
    {
        LoadedObject = obj;

        if (immediateAutoSave)
            Save();
    }

    public WorldObject LoadedObject { get; private set; }

    public static void Create(string name, WorldObject obj)
    {
        WorldObjectPrefab prefab = new(name);
        prefab.LoadedObject = obj;
        prefab.Save();
    }

    /// <summary>
    /// Loads the <see cref="WorldObject"/> into the <see cref="World"/>.
    /// </summary>
    /// <param name="world"></param>
    public WorldObject LoadIn(World world)
    {
        if (LoadedObject == null)
            Load();
        return world.Duplicate(this, LoadedObject.Name);
    }

    /// <summary>
    /// Loads the <see cref="WorldObject"/> from the File. This will overwrite the <see cref="LoadedObject"/> with the loaded object.
    /// <br></br> This will not add the object to the <see cref="World"/>. see <see cref="LoadIn(World)"/> for that.
    /// </summary>
    public override void Load()
    {
        World tempWorld = new("temp world");
        WorldTemplateLoader loader = new(tempWorld);

        loader.LoadTemplate(File.File.FullName);

        LoadedObject = tempWorld.Objects.First();
    }

    /// <summary>
    /// Saves the <see cref="WorldObject"/> to the File.
    /// </summary>
    public override void Save()
    {
        WorldTemplateCreator creator = new();
        string templateData = creator.CreateSaveOf(LoadedObject);

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
    public static object Load(string prefabName)
    {
        var fab = new WorldObjectPrefab(prefabName);
        fab.Load();
        return fab.LoadedObject;
    }

    public static implicit operator WorldObject(WorldObjectPrefab prefab) => prefab.LoadedObject;
}
