using System;
using System.Collections.Generic;
using System.Reflection;
using WinterRose;

namespace WinterRose.Serialization
{
    public static class SnowSerializerHelpers
    {
        /// <summary>
        /// A list of all the types that are supported in a way they do not recursively call the serializer for their properties
        /// 
        /// <br></br><br></br> These are:<br></br>
        /// <see cref="bool"/>, 
        /// <see cref="byte"/>, 
        /// <see cref="sbyte"/>, 
        /// <see cref="char"/>, 
        /// <see cref="decimal"/>, 
        /// <see cref="double"/>,
        /// <see cref="float"/>,<br></br>
        /// <see cref="int"/>, 
        /// <see cref="uint"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="string"/>
        /// </summary>
        public static List<Type> SupportedPrimitives { get; } =
        [
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string)
        ];

        /// <summary>
        /// Gets all the events that are declared in the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="includePrivateFields"></param>
        /// <returns>All the events that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private events</returns>
        public static EventInfo[] GetAllClassEvents<T>(T obj, bool includePrivateFields) =>
            includePrivateFields ? obj.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
            obj.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Gets all the fields that are declared in the specified type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="includePrivateFields"></param>
        /// <param name="assembly"></param>
        /// <returns>All the events that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private events</returns>
        public static EventInfo[] GetAllClassEvents(string typeName, bool includePrivateFields, Assembly? assembly = null) =>
         includePrivateFields ? TypeWorker.FindType(typeName, assembly)?.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        : TypeWorker.FindType(typeName, assembly)?.GetEvents(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Gets all the fields that are declared in the specified type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="includePrivateFields"></param>
        /// <param name="assembly"></param>
        /// <returns>All the fields that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private fields</returns>
        public static FieldInfo[] GetAllClassFields(string typeName, bool includePrivateFields, Assembly? assembly = null) =>
            includePrivateFields ? TypeWorker.FindType(typeName, assembly)?.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            : TypeWorker.FindType(typeName, assembly)?.GetFields(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Gets all fields that are declared in the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="includePrivateFields"></param>
        /// <returns>All the fields that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private fields</returns>
        public static FieldInfo[] GetAllClassFields<T>(T obj, bool includePrivateFields) =>
            includePrivateFields ? obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
            obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Gets all properties that are declared in the specified type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="includePrivateFields"></param>
        /// <param name="assembly"></param>
        /// <returns>All the properties that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private properties</returns>
        public static PropertyInfo[] GetAllClassProperties(string typeName, bool includePrivateFields, Assembly? assembly = null) =>
             includePrivateFields ? TypeWorker.FindType(typeName, assembly)?.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            : TypeWorker.FindType(typeName, assembly)?.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Gets all properties that are declared in the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="includePrivateFields"></param>
        /// <returns>All the properties that are fetched. depending on <paramref name="includePrivateFields"/> it does or doesnt include private properties</returns>
        public static PropertyInfo[] GetAllClassProperties<T>(T obj, bool includePrivateFields) =>
            includePrivateFields ? obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
            obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Extracts the type from the serialzied data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <returns>The type that was extraced</returns>
        public static Type ExtractType(List<string> data, int depth)
        {
            data[0] = data[0].TrimStart('@');
            string depthString = depth.ToString();
            foreach (char c in depthString)
                data[0] = data[0].TrimStart(c);
            string info = data[0].Trim().TrimStart('\0').TrimEnd('\0').Base64Decode();
            data.RemoveAt(0);

            string[] typeassembly = info.Split("--", StringSplitOptions.RemoveEmptyEntries);
            if (typeassembly.Length == 1)
            {
                return ExtractType(info, depth);
            }
            if (typeassembly.Length == 3)
                return TypeWorker.FindType(typeassembly[0], typeassembly[2], typeassembly[1]);
            else
                return TypeWorker.FindType(typeassembly[0], typeassembly[1]);
        }
        /// <summary>
        /// Extracts the type from the serialzied data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <returns>The type that was extracted</returns>
        public static Type ExtractType(string data, int depth)
        {
            data = data.TrimStart('@');
            string depthString = depth.ToString();
            foreach (char c in depthString)
                data = data.TrimStart(c);
            string[] typeassembly = data.Base64Decode().Split("--", StringSplitOptions.RemoveEmptyEntries);

            return TypeWorker.FindType(typeassembly[0], typeassembly[1], typeassembly[2]);
        }
    }
}