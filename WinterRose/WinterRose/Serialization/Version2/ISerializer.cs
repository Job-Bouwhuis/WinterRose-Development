using System.Collections.Generic;
using System.Text;

namespace WinterRose.Serialization.Version2;

/// <summary>
/// Interface for classes that can serialize objects.
/// </summary>
public interface ISerializer : ISerializationWorker
{
    /// <summary>
    /// Serializes the given object into a <see cref="SerializationResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of object wished to be serialzied</typeparam>
    /// <param name="obj">the object that is being serialized</param>
    /// <returns>A <see cref="SerializationResult"/> with the desired fields and properties and their data</returns>
    /// <exception cref="SerializationFailedException"></exception>
    SerializationResult Serialize<T>(T obj);

    /// <summary>
    /// Serializes a collection of objects into a <see cref="SerializationResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of object wished to be serialzied</typeparam>
    /// <param name="collection">The collection of objects to be serialzied</param>
    /// <returns>A <see cref="SerializationResult"/> with the serialized data of all objects in the <paramref name="collection"/></returns>
    /// <exception cref="SerializationFailedException"></exception>
    SerializationResult SerializeCollection<T>(IEnumerable<T> collection);
}