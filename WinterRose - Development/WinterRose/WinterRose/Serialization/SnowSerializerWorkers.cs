using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using WinterRose.AnonymousObjectStuff;
using WinterRose.AnonymousTypes;
using WinterRose.Encryption;
using static WinterRose.Serialization.SnowSerializer;
using static WinterRose.Serialization.SnowSerializerHelpers;

namespace WinterRose.Serialization
{
    /// <summary>
    /// Use only if you know what you are doing, this class is not meant to be used by the end user.
    /// <br></br> it was made public so it can be used by the <see cref="SourceGeneration.Serialization.SerializationGenerator"/> class.
    /// </summary>
    public static class SnowSerializerWorkers
    {
        /// <summary>
        /// Serializes a dictionary
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="depth"></param>
        /// <param name="fieldName"></param>
        /// <returns>A serialized string represetnation of the passed <paramref name="dict"/></returns>
        public static StringBuilder SerializeDictionary(IDictionary dict, int depth, string fieldName, SerializerSettings settings)
        {
            StringBuilder result = new();
            Type[] dicTypes = dict.GetType().GetGenericArguments();
            if (dict.Count < 1)
                return new($"#{depth}{fieldName}|{depth}&{depth}null{DICTIONARY_DEFINER}{depth}null");

            result.Append($"#{depth}{fieldName}|{depth}");

            foreach (DictionaryEntry entry in dict)
            {
                if (dicTypes[0] != null && SupportedPrimitives.Contains(dicTypes[0]))
                {
                    result.Append($"&{depth}{entry.Key}{DICTIONARY_DEFINER}{depth}");
                }
                else
                {
                    result.Append($"&{depth}{SerializeObject(entry.Key, depth + 1, settings)}");
                }

                if (dicTypes[1] != null && SupportedPrimitives.Contains(dicTypes[1]))
                    result.Append((entry.Value == null) ? $"&{depth}null" : entry.Value.ToString());
                else
                {
                    if (entry.Value == null)
                    {
                        result.Append($"{DICTIONARY_DEFINER}{depth}null");
                        continue;
                    }
                    string s = SerializeUnusual(entry.Value, depth
                        );
                    if (!(s is NULL))
                        result.Append(s);
                    else
                        result.Append(SerializeObject(entry.Value, depth + 1, settings));
                }
            }
            return result;
        }
        /// <summary>
        /// Serializes an event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="depth"></param>
        /// <param name="overrideT"></param>
        /// <returns>A serialized string representation of the passed <paramref name="info"/></returns>
        /// <exception cref="SerializationFailedException"></exception>
        public static StringBuilder SerializeEvent<T>(T obj, EventInfo info, int depth, Type? overrideT, SerializerSettings settings)
        {
            Type objectType = (overrideT is not null) ? overrideT : obj.GetType();

            StringBuilder result = new();

            List<EventMethodInfo> methods = new();
            if (info == null)
                throw new SerializationFailedException("Event was not found when attempting to serialize it");

            result.Append($"#{depth}{info.Name}|{depth}");

            EventHelper<T, EventHandler> invocations = new EventHelper<T, EventHandler>(info.Name, obj, new EventHandler((o, a) => { }));

            foreach (EventHandler evnt in invocations.GetInvocationList(obj))
            {
                methods.Add(new EventMethodInfo(evnt.Method.DeclaringType!, evnt.Method.Name));
            }

            if (methods.Count == 0)
                return new($"EVENT{depth}null");

            foreach (var m in methods)
                result.Append($"EVENT{depth}{SerializeObject(m, depth + 1, settings)}");

            return result;
        }
        /// <summary>
        /// Serializes a field or property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="depth"></param>
        /// <param name="settings"></param>
        /// <returns>A serialized string representation of the passed <paramref name="value"/></returns>
        public static StringBuilder SerializeField<T>(T value, string fieldName, Type fieldType, int depth, SerializerSettings settings)
        {
            if (SupportedPrimitives.Contains(fieldType))
            {
                string start = $"#{depth}{fieldName}|{depth}";
                string end;
                if (value is null)
                    end = NULL;
                else if (value is string and "")
                    end = EMPTYSTRING;
                else
                    end = value.ToString();
                return new StringBuilder(start).Append(end);
            }

            if (value is null)
                return new($"#{depth}{fieldName}|{depth}{NULL}");

            string unusual = SerializeUnusual(value, depth);
            if (unusual is not NULL)
                return new StringBuilder($"#{depth}{fieldName}|{depth}{unusual}");

            Attribute? attr = value.GetType().GetCustomAttributes().Where(x => x is SerializeAsAttributeINTERNAL).FirstOrDefault();
            SerializeAsAttributeINTERNAL? serializeAs = attr == null ? null : attr as SerializeAsAttributeINTERNAL;

            if ((value is IEnumerable && serializeAs?.Type == typeof(IEnumerable)) || (value is IEnumerable && serializeAs == null))
            {
                var enumerable = (IEnumerable)value;
                return enumerable is IDictionary dict ?
                    SerializeDictionary(dict, depth, fieldName, settings) :
                    SerializeList(enumerable, depth, fieldName, fieldType, settings);
            }
            if (value.GetType().IsEnum)
            {
                return new($"#{depth}{fieldName}|{depth}{value}");
            }

            StringBuilder result = new();
            result.Append($"#{depth}{fieldName}|{depth}");

            MethodInfo serializeObjectMethod =
                typeof(SnowSerializerWorkers).GetMethod(nameof(SerializeObject), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .MakeGenericMethod(value.GetType());

            result.Append(serializeObjectMethod.Invoke(null, [value, depth + 1, settings]));
            return result;
        }
        /// <summary>
        /// Serializes a collection, such as a list or array
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="depth"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <returns>A serialzied string representation of the passed <paramref name="enumerable"/></returns>
        public static StringBuilder SerializeList(IEnumerable enumerable, int depth, string fieldName, Type fieldType, SerializerSettings settings)
        {
            StringBuilder result = new();
            result.Append($"#{depth}{fieldName}|{depth}");

            Type listItemType = fieldType.IsArray ? fieldType.GetElementType() : fieldType.GetGenericArguments().Single();
            if (enumerable.Count() == 0)
                return result.Append($"^{depth}null");

            foreach (dynamic item in enumerable)
            {
                if (listItemType != null && SupportedPrimitives.Contains(listItemType))
                    result.Append($"^{depth}{item}");
                else
                {
                    string s = SerializeUnusual(item, depth);
                    if (!(s is SnowSerializer.NULL))
                        result.Append(s);
                    else
                    {
                        StringBuilder serialized = SerializeObject(item, depth + 1, settings);
                        if (serialized.Length > 10)
                        {
                            result.Append($"^{depth}");
                            result.Append(serialized);
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Serializes an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="depth"></param>
        /// <param name="overrideT"></param>
        /// <param name="settings"></param>
        /// <returns>A serialized string represetnation of the passed <paramref name="item"/></returns>
        public static StringBuilder SerializeObject<T>(T item, int depth, SerializerSettings settings)
        {
            Type type = typeof(T);
            if (item is IEnumerable)
                type = typeof(T).GetGenericArguments()[0];
            ;
            if (RegisteredSerializers.TryGetValue(type, out RegisteredSerializer? serializer))
            {
                dynamic instance = serializer.GetInstance();
                return instance.Serialize(item, settings, depth);
            }

            bool asAnonymous = typeof(T).IsAnonymousType();
            if (settings.AssumeObjectIsAnonymous && !asAnonymous)
                if (typeof(T) == typeof(object))
                    asAnonymous = true;
            if (typeof(T) == typeof(Anonymous))
                asAnonymous = true;

            if (asAnonymous)
            {
                AnonymousObjectReader reader = new AnonymousObjectReader();
                reader.Read(item);

                return new(reader.Serialize());
            }

            Type objectType = /*(overrideT is not null) ? overrideT : */item.GetType();

            bool includePrivateFields = false;
            if (objectType.CustomAttributes.Any(x => x.AttributeType == typeof(IncludePrivateFieldsAttribute)))
                includePrivateFields = true;
            else if (settings != null && settings.includePrivateFieldsForField)
                includePrivateFields = true;

            StringBuilder result = new();

            if (item == null)
            {
                result.Append($"@{depth}#{depth}null");
                return result;
            }

            var fieldsTemp = GetAllClassFields(item, includePrivateFields);
            var propertiesTemp = GetAllClassProperties(item, includePrivateFields);

            List<FieldInfo> fields = new List<FieldInfo>();
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var field in fieldsTemp)
            {
                if (!field.GetCustomAttributes().Any(x => x.GetType() == typeof(ExcludeFromSerializationAttribute)))
                    fields.Add(field);
            }
            foreach (var property in propertiesTemp)
            {
                if (settings.includePropertiesForField)
                {
                    properties = new List<PropertyInfo>(propertiesTemp);
                    break;
                }
                if (property.GetCustomAttributes().Any(x => x.GetType() == typeof(IncludeWithSerializationAttribute)))
                    properties.Add(property);
            }
            var events = GetAllClassEvents(item, includePrivateFields);

            string typeassembly;
            if (!settings.IncludeType)
            {
                typeassembly = "";
            }
            else if (objectType.Name == "Nullable`1")
            {
                Type t = objectType.GetGenericArguments().First();
                typeassembly = $"@{depth}{$"{t.Name}--{t.Namespace}--{t.Assembly.GetName().FullName}".Base64Encode()}";
            }
            else
                typeassembly = $"{objectType.Name}--{objectType.Namespace}--{objectType.Assembly.GetName().FullName}";

            result.Append($"@{depth}{typeassembly.Base64Encode()}");

            foreach (FieldInfo field in fields)
            {
                result.Append(SerializeField(
                    field.GetValue(item),
                    field.Name,
                    field.FieldType,
                    depth,
                    SerializerSettings.CreateFrom(settings, newSettings =>
                    {
                        newSettings.includePropertiesForField = field.CustomAttributes
                    .Any(x => x.AttributeType == typeof(IncludePropertiesForFieldAttribute));
                        newSettings.includePrivateFieldsForField = field.CustomAttributes
                    .Any(x => x.AttributeType == typeof(IncludePrivateFieldsForFieldAttribute));

                    })));
            }

            foreach (PropertyInfo property in properties)
            {
                result.Append(SerializeField(
                    property.GetValue(item),
                    property.Name,
                    property.PropertyType,
                    depth,
                    SerializerSettings.CreateFrom(settings, newSettings =>
                    {
                        newSettings.includePropertiesForField = property.CustomAttributes
                    .Any(x => x.AttributeType == typeof(IncludePropertiesForFieldAttribute));
                        newSettings.includePrivateFieldsForField = property.CustomAttributes
                    .Any(x => x.AttributeType == typeof(IncludePrivateFieldsForFieldAttribute));

                    })));
            }

            foreach (EventInfo @event in events)
                result.Append(SerializeEvent(item, @event, depth, objectType, settings));

            return result;
        }
        /// <summary>
        /// Serializes an 'unusual' type, such as DateTime, TimeSpan, TimeOnly, DateOnly, such types are serialized in a different way than normal types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="depth"></param>
        /// <returns>A string representation of the passed <paramref name="item"/></returns>
        public static string SerializeUnusual<T>(T item, int depth)
        {
            if(CustomSerializers.TryGetValue(item.GetType(), out ICustomSerializer serializer))
            {
                return serializer.Serialize(item, depth);
            }
            
            return NULL;
        }


        /// <summary>
        /// Deserializes the data into an array of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="listType"></param>
        /// <param name="buffer"></param>
        /// <returns>The deserialized array</returns>
        public static dynamic DeserializeArray<T>(string data, int depth, Type listType, dynamic buffer, SerializerSettings settings)
        {
            DeserializeList<T>(data, depth, listType, buffer, settings);
            return buffer.ToArray();
        }
        /// <summary>
        /// Deserializes the data into a dictionary of the specified types
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        /// <returns>The deserialzied dictionary</returns>
        public static dynamic DeserializeDictionary(string data, int depth, Type keyType, Type valueType, SerializerSettings settings)
        {
            if (data == $"&{depth}null{DICTIONARY_DEFINER}{depth}null")
            {
                Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                return Activator.CreateInstance(dictType);
            }
            List<string> fields = data.Split($"&{depth}", StringSplitOptions.RemoveEmptyEntries).ToList();
            dynamic result = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

            foreach (var fieldData in fields)
            {
                string[] fieldParts;
                if (!fieldData.Contains(DICTIONARY_DEFINER))
                    fieldParts = fieldData.Split($"={depth}", StringSplitOptions.RemoveEmptyEntries);
                else
                    fieldParts = fieldData.Split($"{DICTIONARY_DEFINER}{depth}", StringSplitOptions.RemoveEmptyEntries);
                string key = fieldParts[0];
                string value = fieldParts[1];

                dynamic keyVal = DeserializeField<object>(key, keyType, depth, settings);
                dynamic valueVal = DeserializeField<object>(value, valueType, depth, settings);
                if (keyVal is null)
                {
                    continue;
                }
                result.Add(keyVal, valueVal);
            }
            return result;
        }
        /// <summary>
        /// Deserializes the data into an object of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="fieldType"></param>
        /// <param name="depth"></param>
        /// <returns>The object that was deserialized</returns>
        public static dynamic DeserializeField<T>(string value, Type fieldType, int depth, SerializerSettings settings)
        {
            if (value.StartsWith(EMPTYSTRING))
                return "";
            if (value.StartsWith(NULL))
                return null;
            //if the field is a primitive, set the value
            if (SupportedPrimitives.Contains(fieldType))
                return TypeWorker.TryCastPrimitive(value, fieldType, out dynamic result) ? result : null;

            //try to deserialze it as a unusual supported type.
            dynamic unusual = SnowSerializerWorkers.DeserializeUnusual(value, depth, fieldType);
            //if that succeeded, then return that value, otherwise continue as normal
            if (unusual is not string and not NULL)
                return unusual;


            //if the field is a list, deserialize the list
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = fieldType.GetGenericArguments().Single();
                var result = WinterUtils.CreateList(listType);
                return DeserializeList<T>(value, depth, listType, result, settings);
            }

            //if the field is an array, deserialize the array
            if (fieldType.IsArray)
            {
                Type arrayType = fieldType.GetElementType();

                var result = WinterUtils.CreateList(arrayType);

                return DeserializeArray<T>(value, depth, arrayType, result, settings);
            }

            //if the field is a dictionary, deserialize the dictionary
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] dicTypes = fieldType.GetGenericArguments();
                return SnowSerializerWorkers.DeserializeDictionary(value, depth, dicTypes[0], dicTypes[1], settings);
            }

            if (fieldType.IsEnum)
            {
                return Enum.Parse(fieldType, value);
            }

            if (fieldType == typeof(EventHandler))
            {
                return null;
            }

            //if none of these were selected then try to deserialize it as a class/struct. an error will be thrown should this fail
            return DeserializeObject<T>(value, depth + 1, fieldType, settings);
        }
        /// <summary>
        /// Deserializes the data into a list of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="listType"></param>
        /// <param name="buffer"></param>
        /// <returns>The deserialized list</returns>
        public static dynamic DeserializeList<T>(string data, int depth, Type listType, IList buffer, SerializerSettings settings)
        {
            //split the data into individual items
            string[] values = data.Split($"^{depth}", StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in values)
            {
                object e = DeserializeField<T>(value, listType, depth, settings);
                if (e is not null and (not string and not NULL))
                    buffer.Add(e);
            }
            return buffer;
        }
        /// <summary>
        /// Deserializes the data into an object of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="overrideT"></param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="DeserializationFailedException"></exception>
        /// <exception cref="FieldNotFoundException"></exception>
        public static dynamic DeserializeObject<T>(string data, int depth, Type? overrideT, SerializerSettings settings)
        {
            Type objectType = overrideT ?? typeof(T);
            List<string> fields = data.Split($"#{depth}", StringSplitOptions.RemoveEmptyEntries).ToList();
            bool includePrivateFields = false;

            bool asAnonymous = typeof(T).IsAnonymousType();
            if (settings.AssumeObjectIsAnonymous && !asAnonymous)
                if (typeof(T) == typeof(object))
                    asAnonymous = true;
            if (typeof(T) == typeof(Anonymous))
                asAnonymous = true;

            if (asAnonymous)
            {
                AnonymousObjectReader reader = new();
                return reader.Deserialize(data);
            }

            if (settings.IncludeType)
            {
                if (objectType == typeof(EventMethodInfo))
                    fields.RemoveAt(0);
                else if (fields[0].StartsWith('@'))
                {
                    Type extracted = ExtractType(fields, depth);
                    if (objectType != extracted && extracted != null)
                        objectType = extracted;
                }
            }
            else if (fields[0] == $"@{depth}")
            {
                fields.RemoveAt(0);
            }


            Type type = typeof(T);
            if (typeof(T).IsAssignableTo(typeof(IEnumerable)))
                type = typeof(T).GetGenericArguments()[0];
            ;
            if (RegisteredSerializers.TryGetValue(type, out RegisteredSerializer? serializer))
            {
                dynamic instance = serializer.GetInstance();
                return instance.Deserialize(data, settings, depth);
            }

            if (objectType.CustomAttributes.Any(x => x.AttributeType == typeof(IncludePrivateFieldsAttribute)))
                includePrivateFields = true;

            //create a new instance of the object
            dynamic result;
            try
            {
                result = Activator.CreateInstance(objectType);
            }
            catch (Exception e)
            {
                result = ActivatorExtra.CreateInstance(objectType);
            }
            if (result is null)
                throw new DeserializationFailedException("Could not create instance of type " + objectType.FullName);

            TypedReference reference = new TypedReference();
            if (!objectType.IsClass)
                reference = __makeref(result);
            if (fields.Count is 0)
                return result;

            //loop through the fields and set the values
            foreach (var fieldData in fields)
            {
                object value;

                //split the field into name and value
                string[] fieldParts = ((string)fieldData).Split($"|{depth}");
                string fieldName = fieldParts[0];
                string fieldValue = fieldParts[1];

                if (fieldValue.StartsWith("EVENT"))
                {
                    string[] methods = ((string)fieldValue).Split($"EVENT{depth}", StringSplitOptions.RemoveEmptyEntries);

                    var evnt = objectType.GetEvent(fieldName);
                    foreach (string m in methods)
                    {
                        EventMethodInfo info = DeserializeObject<EventMethodInfo>(m, depth + 1, typeof(EventMethodInfo), settings);
                        MethodInfo method = TypeWorker.FindMethod(TypeWorker.FindType(info.typeName, info.typeAssembly.Base64Decode()), info.methodName);
                        Delegate handler = Delegate.CreateDelegate(evnt.EventHandlerType, null, method);
                        evnt.AddEventHandler(result, handler);
                    }
                    continue;
                }

                //get field info with the name from the serialized data
                FieldInfo fieldInfo = objectType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                PropertyInfo propertyInfo = objectType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo == null && propertyInfo == null)
                {
                    if (settings.IgnoreNotFoundFields)
                        continue;
                    else
                        throw new FieldNotFoundException($"Field {fieldName} not found in type {objectType.Name}");
                }

                Type fieldType;
                fieldType = fieldInfo == null ? propertyInfo!.PropertyType : fieldInfo.FieldType;

                value = DeserializeField<T>(fieldValue, fieldType, depth, settings);
                ;

                if (fieldInfo != null)
                    if (objectType.IsClass)
                        fieldInfo.SetValue(result, value);
                    else
                        fieldInfo.SetValueDirect(reference, value);
                else
                    propertyInfo!.SetValue(result, value);


                fieldParts = null;
                fieldName = null;
                fieldValue = null;
            }
            return result;

        }
        /// <summary>
        /// Deserializes an 'unusual' type, such as DateTime, TimeSpan, TimeOnly, DateOnly, such types are serialized in a different way than normal types
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <param name="fieldType"></param>
        /// <returns>The value that was deserialized</returns>
        public static dynamic DeserializeUnusual(string data, int depth, Type fieldType)
        {
            if(CustomSerializers.TryGetValue(fieldType, out var result))
            {
                return result.Deserialize(data, depth);
            }

            return NULL;
        }
    }
}
