using System.Collections.Generic;

namespace WinterRose.Serialization.Version2;

/// <summary>
/// Interface for classes that can deserialize objects.
/// </summary>
public interface IDeserializer : ISerializationWorker
{
    /// <summary>
    /// Deserializes the given data into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object that will be attempted to be created</typeparam>
    /// <param name="data">The data where the values of all the fields and properties will be taken from</param>
    /// <returns>A new Object of type <typeparamref name="T"/> with the data found in <paramref name="data"/></returns>
    /// <exception cref="DeserializationFailedException">Throws an exception if the data is not in the correct format</exception>
    DeserializationResult Deserialize<T>(string data);

    /// <summary>
    /// Deseriallize the given <paramref name="data"/> into a collection of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object that will be attempted to be created</typeparam>
    /// <param name="data">The data where the values of all the fields and properties will be taken from</param>
    /// <returns>A <see cref="IEnumerable{T}"/> containing all items found in <paramref name="data"/></returns>
    ///     /// <exception cref="DeserializationFailedException">Throws an exception if the data is not in the correct format</exception>
    DeserializationResult DeserializeCollection<T>(string data);
}