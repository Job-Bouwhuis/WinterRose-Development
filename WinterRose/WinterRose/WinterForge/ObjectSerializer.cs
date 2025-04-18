using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using WinterRose.Serialization;
using WinterRose;
using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WinterRose.WinterForge
{
    public class ObjectSerializer
    {
        private readonly Dictionary<object, int> cache = [];

        private int currentKey = 0;

        // Serialize an object to the given stream
        public void Serialize(object obj, Stream destinationStream, bool isRootCall)
        {
            if (isRootCall)
            {
                cache.Clear();
                currentKey = 0;
            }

            if (obj == null)
            {
                WriteToStream(destinationStream, "null");
                return;
            }

            if (cache.TryGetValue(obj, out int key))
            {
                WriteToStream(destinationStream, $"_ref({key})");
                return;
            }
            else
                key = currentKey++;

            Type objType = obj.GetType();

            if (SnowSerializerHelpers.SupportedPrimitives.Contains(objType))
            {
                WriteToStream(destinationStream, obj.ToString());
                return;
            }

            string? collection = TryCollection(obj);
            if (collection is not null)
            {
                WriteToStream(destinationStream, collection);
                return;
            }

            if (CustomValueProviderCache.Get(objType, out var provider))
            {
                WriteToStream(destinationStream, provider._CreateString(obj));
                return;
            }

            // Write the type and an index for the object
            WriteToStream(destinationStream, $"{objType.FullName} : {key} {{\n");

            // Use reflection to get properties and fields and serialize them
            var helper = new ReflectionHelper(ref obj);

            // dont cache the object if it doesnt wish to be cached. or is a struct
            if (objType.GetCustomAttributes<NotCachedAttribute>().FirstOrDefault() is null)
                if (!objType.IsValueType)
                    cache.Add(obj, key);

            SerializePropertiesAndFields(obj, helper, destinationStream);

            WriteToStream(destinationStream, "}\n");

            if (isRootCall)
            {
                WriteToStream(destinationStream, "\n\nreturn " + key);
            }
            destinationStream.Flush();
        }

        // Serialize properties and fields of an object directly to the stream
        private void SerializePropertiesAndFields(object obj, ReflectionHelper rh, Stream destinationStream)
        {
            Type objType = obj.GetType();

            bool hasIncludeAllProperties =
                objType.GetCustomAttributes<IncludeAllPropertiesAttribute>().FirstOrDefault() is not null;
            bool hasIncludeAllPrivateFields =
                objType.GetCustomAttributes<IncludePrivateFieldsAttribute>().FirstOrDefault() is not null;



            // Serialize properties
            var members = rh.GetMembers();
            foreach (var member in members)
            {
                if (member.IsStatic)
                    continue;
                if (!member.CanWrite)
                    continue; // ignore unwritable members, cant restore them anyway
                if (!member.IsPublic)
                {
                    if (!member.Attributes.Any(x => x is IncludeWithSerializationAttribute) && !hasIncludeAllPrivateFields)
                        continue;
                    if (member.Name.Contains('<') && hasIncludeAllPrivateFields)
                        continue; // skip property backing fields anyway regardless of private field setting
                }
                if (member.Attributes.Any(x => x is ExcludeFromSerializationAttribute))
                    continue;

                if (member.MemberType == MemberTypes.Property)
                {
                    if (!member.Attributes.Any(x => x is IncludeWithSerializationAttribute) && !hasIncludeAllProperties)
                        continue;
                }


                // if value cant be written to, dont bother saving its value. cant be restored anyway
                if (member.CanWrite)
                {
                    CommitValue(obj, destinationStream, member);
                }
            }
        }

        private void CommitValue(object obj, Stream destinationStream, MemberData member)
        {
            object value = member.GetValue(obj);

            string serializedString = SerializeValue(value);
            int linePos = serializedString.IndexOf('\n');
            if (linePos != -1)
            {
                string line = serializedString[0..linePos];
                linePos = line.IndexOf(':');
                if (linePos != -1)
                {
                    ReadOnlySpan<char> indexStart = line.AsSpan()[linePos..];
                    int len = indexStart.Length;

                    // Trim from the end while chars are not numeric
                    while (len > 0 && !char.IsDigit(indexStart[len - 1]))
                        len--;

                    indexStart = indexStart[1..len].Trim();
                    int key = int.Parse(indexStart);

                    WriteToStream(destinationStream, serializedString);
                    WriteToStream(destinationStream, $"{member.Name} = _ref({key});\n");
                    return;
                }
            }

            WriteToStream(destinationStream, $"{member.Name} = {serializedString}");
            if (!serializedString.Contains('['))
            {
                WriteToStream(destinationStream, ";\n");
            }
        }

        // Serialize individual values directly, handling nested objects
        private string SerializeValue(object value)
        {
            if (value == null)
                return "null";

            Type valueType = value.GetType();

            // Check if the value is a primitive or string
            if (valueType.IsPrimitive || value is string)
                return value.ToString();

            // Handle arrays, lists, and collections (nested objects)
            string? collection = TryCollection(value);
            if (collection is not null)
                return collection;

            // If the value is a nested object, recursively serialize it
            return RecursiveSerialization(value); // We can reuse the same serializer method for nested objects
        }

        private string? TryCollection(object value)
        {
            if (value is Array array)
            {
                StringBuilder arrayString = new StringBuilder();
                arrayString.Append("[");
                foreach (var item in array)
                {
                    arrayString.Append(SerializeValue(item) + ", ");
                }
                arrayString.Append("]");
                return arrayString.ToString();
            }

            SerializeAsAttributeINTERNAL? attr
                = value.GetType().GetCustomAttribute<SerializeAsAttributeINTERNAL>();
            if (attr is not null && attr.Type != typeof(IEnumerable))
                return null;

            if (value is IEnumerable list)
            {
                StringBuilder listString = new StringBuilder();

                Type listType = value.GetType().GetGenericArguments()[0];

                listString.Append($"<{listType.FullName}>[\n");
                bool firstItem = true;
                foreach (var item in list)
                {
                    if (!firstItem)
                        listString.Append(",\n");

                    var v = SerializeValue(item);
                    listString.Append(v);
                    firstItem = false;
                }
                listString.Append("]\n");
                return listString.ToString();
            }

            return null;
        }

        // Write the serialized content directly to the stream
        private void WriteToStream(Stream stream, string content)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            stream.Write(contentBytes, 0, contentBytes.Length);
        }

        // Helper method to get a string representation of the serialized object
        private string RecursiveSerialization(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serialize(obj, ms, false);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Serializes the given object to a string using the <see cref="WinterForge"/> serialization system
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string SerializeToString(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serialize(obj, ms, true);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        // Helper method to directly write to a file
        public void SerializeToFile(object obj, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Serialize(obj, fs, true);
            }
        }
    }
}
