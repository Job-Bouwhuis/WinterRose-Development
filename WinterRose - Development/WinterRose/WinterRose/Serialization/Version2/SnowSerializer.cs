using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.Version2;

/// <summary>
/// Provides methods to serialize and deserialize objects.
/// </summary>
public class SnowSerializer
{
    /// <summary>
    /// The default instance of the <see cref="SnowSerializer"/> class. can be used to quickly serialize and deserialize objects.<br></br><br></br>
    /// 
    /// if you wish to use a custom <see cref="ISerializer"/> or <see cref="IDeserializer"/> use the <see cref="SnowSerializer(ISerializer, IDeserializer)"/> constructor.
    /// <br></br><br></br>
    /// Use the constructor  to use custom <see cref="SerializeOptions"/>.<br></br><br></br>
    /// </summary>
    public static readonly SnowSerializer Default = new();
    public SerializeOptions SerializeOptions { get; private set; } = new();
    public ISerializer Serializer { get; init; }
    public IDeserializer Deserializer { get; init; }
    /// <summary>
    /// Subscribes the provided action to the Serializers, and Deserializers Logger events.
    /// </summary>
    public event Action<ProgressLog> Logger
    {
        add
        {
            Serializer.Logger += value;
            Deserializer.Logger += value;
        }
        remove
        {
            Serializer.Logger -= value;
            Deserializer.Logger -= value;
        }
    }
    /// <summary>
    /// Subscribes the provided action to the Serializers, and Deserializers AsyncLogger events.
    /// </summary>
    public event Action<ProgressLog> AsyncLogger
    {
        add
        {
            Serializer.AsyncLogger += value;
            Deserializer.AsyncLogger += value;
        }
        remove
        {
            Serializer.AsyncLogger -= value;
            Deserializer.AsyncLogger -= value;
        }
    }

    /// <summary>
    /// Subscribes the provided action to the Serializers, and Deserializers SimpleLogger events.
    /// </summary>
    public event Action<ProgressLog> SimpleLogger
    {
        add
        {
            Serializer.SimpleLogger += value;
            Deserializer.SimpleLogger += value;
        }
        remove
        {
            Serializer.SimpleLogger -= value;
            Deserializer.SimpleLogger -= value;
        }
    }

    public SnowSerializer(Action<SerializeOptions> options)
    {
        SerializeOptions ops = new();
        options(ops);
        SerializeOptions = ops;
    }

    public SnowSerializer() : this(_ => { })
    {
        Serializer = new DefaultSerializer(SerializeOptions);
        Deserializer = new DefaultDeserializer(SerializeOptions);
    }

    public SnowSerializer(ISerializer serializer, IDeserializer deserializer) : this()
    {
        Serializer = serializer;
        Deserializer = deserializer;
    }
    public SnowSerializer(ISerializer serializer, IDeserializer deserializer, Action<SerializeOptions> options) : this(options)
    {
        Serializer = serializer;
        Deserializer = deserializer;
    }

    /// <summary>
    /// Calls the <see cref="ISerializer.Serialize{T}(T)"/> method of the <see cref="Serializer"/> property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public SerializationResult Serialize<T>(T obj)
    {
        Serializer.Resume();
        if (obj is IEnumerable collection)
        {
            MethodInfo method = Serializer.GetType().GetMethod(nameof(Serializer.SerializeCollection));
            MethodInfo generic = method.MakeGenericMethod(obj.GetType().GetGenericArguments()[0]);
            return (SerializationResult)generic.Invoke(Serializer, new object[] { obj });
        }
        return Serializer.Serialize(obj);
    }

    /// <summary>
    /// Calls the <see cref="IDeserializer.Deserialize{T}(string)"/> method of the <see cref="Deserializer"/> property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serialzied"></param>
    /// <returns>The result of the deserialization</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public DeserializationResult Deserialize<T>(string serialzied)
    {
        Deserializer.Resume();
        if (typeof(T).IsAssignableTo(typeof(IEnumerable)))
        {
            MethodInfo method = Deserializer.GetType().GetMethod(nameof(Deserializer.DeserializeCollection));
            MethodInfo generic = method.MakeGenericMethod(typeof(T).GetGenericArguments()[0]);
            //Type genericEnumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
            dynamic deserializationResult = generic.Invoke(Deserializer, new object[] { serialzied });
            Type type = Type.GetType($"System.Collections.Generic.List`1[[{typeof(T).GetGenericArguments()[0].AssemblyQualifiedName}]]");
            ConstructorInfo[] constructors = type.GetConstructors();
            return new(Activator.CreateInstance(type, deserializationResult.Result));
        }
        return Deserializer.Deserialize<T>(serialzied);
    }

    public void Abort()
    {
        Serializer.Abort();
        Deserializer.Abort();
    }

    /// <summary>
    /// Sets the new options for this <see cref="SnowSerializer"/> instance.
    /// </summary>
    /// <param name="options"></param>
    /// <returns>The same instance of the <see cref="SnowSerializer"/></returns>
    public SnowSerializer WithOptions(Action<SerializeOptions> options)
    {
        options(SerializeOptions);;
        Serializer.Options = SerializeOptions;
        Deserializer.Options = SerializeOptions;
        return this;
    }




}