using WinterRose.Monogame.Animations;

namespace WinterRose.Monogame;

/// <summary>
/// Base class for all things that can be loaded to the game world (such as <see cref="WorldObject"/>, <see cref="AnimationKey"/>, or for example a path object for a tower defence game)
/// </summary>
public abstract class Prefab : Asset
{
    /// <summary>
    /// Creates a new instance of <see cref="Prefab"/>
    /// </summary>
    /// <param name="name"></param>
    public Prefab(string name) : base(name) { }

    /// <summary>
    /// Unloads the prefab
    /// </summary>
    public override abstract void Unload();
    /// <summary>
    /// Loads the prefab
    /// </summary>
    public override abstract void Load();

    /// <summary>
    /// Sabes the prefab
    /// </summary>
    public override abstract void Save();

    /// <summary>
    /// Creates a new instance of <see cref="Prefab"/> from a file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T Create<T>(string name) where T : Prefab
    {
        return AssetDatabase.LoadAsset<T>(name);
    }
}
