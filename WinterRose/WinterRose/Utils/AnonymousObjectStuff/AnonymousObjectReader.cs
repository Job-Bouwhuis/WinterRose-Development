using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using WinterRose.Serialization;
using System.Linq;
using System;

namespace WinterRose.AnonymousTypes;

/// <summary>
/// A class that can read and write anonymous objects.
/// </summary>
public class AnonymousObjectReader : DynamicObject
{
    private readonly Dictionary<string, object> map = [];
    private readonly Dictionary<string, AnonymousMethod> methods = new();

    /// <summary>
    /// Gets the dynamic member names.
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        List<string> names = new List<string>();
        foreach (var pair in map)
            names.Add(pair.Key);
        foreach (var pair in methods)
            names.Add(pair.Key);
        return names;
    }

    /// <summary>
    /// Creates a new empty anonymous object reader.
    /// </summary>
    public AnonymousObjectReader()
    {
    }

    /// <summary>
    /// Creates a new anonymous object reader from the specified object.
    /// </summary>
    /// <param name="obj"></param>
    public AnonymousObjectReader(object obj)
    {
        Read(obj);
    }

    /// <summary>
    /// Tries to get the member.
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var value = this[binder.Name];
        result = value.Value;
        return value.HasValue;
    }

    /// <summary>
    /// Adds a new property to the object.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Add(string name, object value)
    {
        map[name] = value;
    }

    /// <summary>
    /// Gets or sets a property value.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public AnonymousValue this[string name]
    {
        get
        {
            if (map.TryGetValue(name, out var value))
                return new AnonymousValue(name, value);
            if (methods.TryGetValue(name, out var method))
                return new AnonymousValue(name, method);

            return new AnonymousValue(name, null);
        }
        set => map[name] = value;
    }

    /// <summary>
    /// Gets or sets a property value by index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object this[int index]
    {
        get => map.ElementAt(index).Value;
        set => map[map.ElementAt(index).Key] = value;
    }

    /// <summary>
    /// The number of properties in the object.
    /// </summary>
    public int Count => map.Count;

    /// <summary>
    /// Gets the enumerator of all properties.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, object>> GetEnumerator() => map;

    /// <summary>
    /// Reads an anonymous object. Overrides the current object.
    /// </summary>
    /// <param name="obj"></param>
    public void Read(object obj)
    {
        map.Clear();
        // obj is an anonymous object, so we need to use reflection to get the properties
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.PropertyType.IsAnonymousType())
            {
                var reader = new AnonymousObjectReader();
                reader.Read(property.GetValue(obj));
                Add(property.Name, reader);
            }
            else
            {
                Add(property.Name, property.GetValue(obj));
            }
        }

        var fields = obj.GetType().GetFields();
        foreach (var field in fields)
        {
            if (field.FieldType.IsAnonymousType())
            {
                var reader = new AnonymousObjectReader();
                reader.Read(field.GetValue(obj));
                Add(field.Name, reader);
            }
            else
            {
                Add(field.Name, field.GetValue(obj));
            }
        }

        var methods = obj.GetType().GetMethods();
        foreach (var method in methods)
        {
            if (method.Name.StartsWith("set_") 
                || method.Name.StartsWith("get_") 
                || method.Name.StartsWith("add_") 
                || method.Name.StartsWith("remove_")
                || method.Name.StartsWith("op_Implicit"))
                continue;
            this.methods.Add(method.Name, new(method.Name, obj, method, method.GetParameters().Select(p => p.ParameterType).ToArray()));
        }
    }

    /// <summary>
    /// Parses the object into a string.
    /// </summary>
    /// <param name="objectDepth"></param>
    /// <returns></returns>
    public string Serialize(int objectDepth = 0)
    {
        // serialize into format "propertyName1=value1;propertyName2=value2"
        // keep in mind that there can be nested anonymous objects
        // keep track of the depth of the object to make sure we can reassign the correct values to the correct properties.
        // instead of adding all properties to a single expando object when we are deserializing,
        // we can keep track of the depth of the object and assign the correct values to the correct properties.

        var sb = new StringBuilder();
        foreach (var pair in map)
        {
            if (pair.Value is not AnonymousObjectReader && !SnowSerializerHelpers.SupportedPrimitives.Contains(pair.Value.GetType()))
            {
                MethodInfo[] methods = typeof(SnowSerializer).GetMethods();
                MethodInfo SerializeMethod = methods.First(m => m.Name == "Serialize" && m.GetParameters().Length == 2);
                dynamic serialziedResult = SerializeMethod.MakeGenericMethod(pair.Value.GetType()).Invoke(null, new object[] { pair.Value, SnowSerializer.DefaultSettings });
                string value = serialziedResult.Result;
                sb.Append($"{pair.Key}={objectDepth}|{$"{pair.Value.GetType().Namespace}.{pair.Value.GetType().Name}".Base64Encode()}|{value.Base64Encode()};{objectDepth}");
            }
            else if (pair.Value is AnonymousObjectReader reader)
                sb.Append($"{pair.Key}={objectDepth}{reader.Serialize(objectDepth + 1)};{objectDepth}");
            else
                sb.Append($"{pair.Key}={objectDepth}|{$"{pair.Value.GetType().Namespace}.{pair.Value.GetType().Name}".Base64Encode()}|{pair.Value};{objectDepth}");
        }
        return sb.ToString();
    }

    public object Deserialize(string data, int objectDepth = 0)
    {
        // deserialize the string into an AnnonymousObjectReader
        // keep track of the depth of the object to make sure we can reassign the correct values to the correct properties.
        // instead of adding all properties to a single expando object when we are deserializing,
        // we can keep track of the depth of the object and assign the correct values to the correct properties.

        var reader = new AnonymousObjectReader();
        var pairs = data.Split($";{objectDepth}", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split($"={objectDepth}");
            var key = keyValue[0];
            object value = keyValue[1];
            if (((string)value).Contains($"={objectDepth + 1}"))
            {
                reader.Add(key, Deserialize((string)value, objectDepth + 1));
            }
            else
            {
                var typeValue = ((string)value).Split("|", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var type = Type.GetType(typeValue[0].Base64Decode());
                value = typeValue[1];

                if (SnowSerializerHelpers.SupportedPrimitives.Contains(type))
                {
                    value = TypeWorker.CastPrimitive(value, type);
                }
                else
                {
                    MethodInfo DeserializeMethod = typeof(SnowSerializer).GetMethod("Deserialize", 1, [typeof(string), typeof(SerializerSettings)]);
                    value = DeserializeMethod.MakeGenericMethod(type).Invoke(null, new object[] { ((string)value).Base64Decode(), SnowSerializer.DefaultSettings });
                    value = ((dynamic)value).Result;
                }

                reader.Add(key, value);
            }
        }

        return CreateAnonymousObject(reader.map);
    }

    /// <summary>
    /// Creates an anonymous object from the specified properties.
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static object CreateAnonymousObject(Dictionary<string, object> properties)
    {
        // Define properties dynamically
        var typeBuilder = AnonymousTypeBuilder.CreateNewAnonymousType(properties);
        var obj = Activator.CreateInstance(typeBuilder);

        // Set property values
        foreach (var property in properties)
            obj.GetType().GetProperty(property.Key).SetValue(obj, property.Value);

        return obj;
    }
}
