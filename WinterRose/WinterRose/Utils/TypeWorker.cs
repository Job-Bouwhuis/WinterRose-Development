using WinterRose.Serialization;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;
using WinterRose.Exceptions;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WinterRose
{
    /// <summary>
    /// Provides methods for finding a type or method within accessable assembiles, and easy casting to and from default data types
    /// </summary>
    public static class TypeWorker
    {
        /// <summary>
        /// WIP method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetBaseTypesWIP(this Type type)
        {
            if (type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypesWIP))
                             .Concat(type.BaseType.GetBaseTypesWIP());
        }

        /// <summary>
        /// searches for the method that implicitly converts the source type to the target type. Works with generic type conversions.
        /// 
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static MethodInfo? FindImplicitConversionMethod(Type targetType, Type sourceType)
        {
            // Check for implicit operators in both the source and target types
            var conversionMethod = sourceType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "op_Implicit" &&
                                     m.ReturnType == targetType &&
                                     m.GetParameters()[0].ParameterType == sourceType);

            if (conversionMethod == null)
            {
                conversionMethod = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "op_Implicit" &&
                                         m.ReturnType == targetType &&
                                         m.GetParameters()[0].ParameterType == sourceType);
            }

            return conversionMethod;
        }

        /// <summary>
        /// the same as Convert.ChangeType(object frrom, object To) except this returns a dynamic instead of an object
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>a dynamic object converted to the given type. or null if converting failed</returns>
        public static dynamic Convert(dynamic from, Type to) => System.Convert.ChangeType(from, to);
        /// <summary>
        /// Searches for the Type matching to the given name. can pass the assembly as filter for the search
        /// </summary>
        /// <returns>The type matching the given name if it is found within the current accessable assemblies. if no matching type is found it returns null</returns>
        public static Type FindType(string typeName, Assembly? targetAssembly = null)
        {
            Type? type = null;
            if (targetAssembly == null)
                AppDomain.CurrentDomain.GetAssemblies().Foreach(x => x.GetTypes().Foreach(x => { if (x.Name == typeName || x.FullName == typeName) type = x; }));
            else
                targetAssembly.GetTypes().Foreach(x => { if (x.Name == typeName || x.FullName == typeName) type = x; });
            return type;
        }
        /// <summary>
        /// Searches for the Type matching to the given name. can pass the assembly as filter for the search. be sure to just give the name of the assembly
        /// </summary>
        /// <returns>The type matching the given name if it is found within the given Assembly. if no matching type is found it returns null</returns>
        public static Type? FindType(string typeName, string targetAssemblyName)
        {
            Type? type = null;
            Assembly? assembly = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            assemblies.Foreach(x => { if (x.FullName.StartsWith(targetAssemblyName)) assembly = x; });
            assembly?.GetTypes()
            .Foreach(x => { if (x.Name == typeName) type = x; });
            return type;
        }
        /// <summary>
        /// Finds all types that have the given attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An array of types that have the given attribute. if no type has the given attribute it returns an empty array</returns>
        public static Type[] FindTypesWithAttribute<T>() where T : Attribute
        {
            List<Type> types = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //see if the type matches the type name, and exists within the same namespace as given with the parameter
            assemblies.Foreach(x => x.GetTypes().Where(t => t.GetCustomAttributes().Any(a => a.GetType() == typeof(T))).Foreach(vt => types.Add(vt)));
            return types.ToArray();
        }

        public static Type[] FindTypesWithBase<T>()
        {
            List<Type> types = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();
            foreach(Type type in allTypes)
            {
                if(type.IsAssignableTo(typeof(T)))
                    types.Add(type);
            }
            return [.. types];
        }

        public static Type[] FindTypesWithInterface<T>()
        {
            List<Type> types = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //see if the type matches the type name, and exists within the same namespace as given with the parameter
            assemblies.Foreach(x => x.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(T))).Foreach(t => types.Add(t)));
            return [.. types];
        }

        public static Type[] FindTypesWithInterface(Type type)
        {
            List<Type> types = [];
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // see if the type implements the given interface, if it is a generic interface, check if the provided type specifies the generic type
            // if it does, check if the generic type matches the generic type of the interface
            // otherwise, just check if the type implements the interface

            assemblies.Foreach(x => 
            x.GetTypes().Where(t => 
            t.GetInterfaces().Any(i => i == type || (type.IsGenericType && i.IsGenericType && i.GetGenericTypeDefinition() == type.GetGenericTypeDefinition())))
            .Foreach(t => types.Add(t)));

            return [.. types];
        }

        /// <summary>
        /// Searches for the Type matching to the given name. can pass the assembly as filter for the search. be sure to just give the name of the assembly
        /// </summary>
        /// <returns>The type matching the given name if it is found within the given Assembly and namespace. if no matching type is found it returns null</returns>
        public static Type? FindType(string typeName, string assemblyName, string @namespace)
        {
            Type? type = null;
            Assembly? assembly = null;
            
            //get all the available assemblies
            AppDomain.CurrentDomain.GetAssemblies().Foreach(x => { if (x.FullName.StartsWith(assemblyName)) assembly = x; });
            
            //see if the type matches the type name, and exists within the same namespace as given with the parameter
            assembly?.GetTypes() .Foreach(x => { if (x.Name == typeName && x.Namespace == @namespace) type = x; });
            
            //return the found type, or null if none were found
            return type;
        }

        /// <summary>
        /// Searches for a method within the given type that has the given name.
        /// </summary>
        /// <param name="containing"></param>
        /// <param name="name"></param>
        /// <returns>returns the found method info if a method with the given name is found, otherwise returns null</returns>
        public static MethodInfo? FindMethod(Type containing, string name)
        {
            var methodsInType = containing.GetMethods();
            foreach (MethodInfo method in methodsInType)
            {
                if (method.Name == name)
                    return method;
            }
            return null;
        }

        /// <summary>
        /// Casts the given file to the given destination type. should this fail it throws an exception
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="targetAssembly"></param>
        /// <param name="targetTypeName"></param>
        /// <returns>the converted value</returns>
        /// <exception cref="TypeNotFoundException"></exception>
        /// <exception cref="FailedToCastTypeException"></exception>
        /// <exception cref="CastTypeNotSupportedException"></exception>
        public static dynamic CastPrimitive(dynamic from, Type? to, Assembly? targetAssembly = null, string? targetTypeName = null)
        {
            Type baseObjectType;
            if (!(targetAssembly is null) && !(targetTypeName is null))
            {
                Type? t = FindType(targetTypeName, targetAssembly);
                if (!(t is null))
                    baseObjectType = t;
                else
                    throw new TypeNotFoundException("could not find type while deserializing object.");
            }
            else
            {
                baseObjectType = to;
            }

            dynamic result;
            if (baseObjectType == typeof(string))
            {
                result = from.ToString();
            }
            else if (baseObjectType == typeof(int))
            {
                if (!int.TryParse($"{from}", out int i))
                    throw new FailedToCastTypeException("could not cast source to int");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(float))
            {
                if (!float.TryParse($"{from}", out float i))
                    throw new FailedToCastTypeException("could not cast source to float");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(double))
            {
                if (!double.TryParse($"{from}", out double i))
                    throw new FailedToCastTypeException("could not cast source to double");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(long))
            {
                if (!long.TryParse($"{from}", out long i))
                    throw new FailedToCastTypeException("could not cast source to long");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(bool))
            {
                if (!bool.TryParse($"{from}", out bool i))
                {
                    if (from == 1)
                        i = true;
                    else if (from == 0)
                        i = false;
                    else
                        throw new FailedToCastTypeException("could not cast source to bool");
                    result = i;
                }
                else
                    result = i;
            }
            else if (baseObjectType == typeof(short))
            {
                if (!short.TryParse($"{from}", out short i))
                    throw new FailedToCastTypeException("could not cast source to short");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(byte))
            {
                if (!byte.TryParse($"{from}", out byte i))
                    throw new FailedToCastTypeException("could not cast source to byte");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(char))
            {
                if (!char.TryParse($"{from}", out char i))
                    throw new FailedToCastTypeException("could not cast source to char");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(sbyte))
            {
                if (!sbyte.TryParse($"{from}", out sbyte i))
                    throw new FailedToCastTypeException("could not cast source to sByte");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(decimal))
            {
                if (!decimal.TryParse($"{from}", out decimal i))
                    throw new FailedToCastTypeException("could not cast source to decimal");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(uint))
            {
                if (!uint.TryParse($"{from}", out uint i))
                    throw new FailedToCastTypeException("could not cast source to uInt");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(ulong))
            {
                if (!ulong.TryParse($"{from}", out ulong i))
                    throw new FailedToCastTypeException("could not cast source to ulong");
                else
                    result = i;
            }
            else if (baseObjectType == typeof(ushort))
            {
                if (!ushort.TryParse($"{from}", out ushort i))
                    throw new FailedToCastTypeException("could not cast source to ushort");
                else
                    result = i;
            }
            else
                throw new CastTypeNotSupportedException("given type to cast to is not supported");

            if (result is not null)
                return result;
            else
                throw new FailedToCastTypeException("result failed to assign.");
        }
        /// <summary>
        /// Casts the given file to the given destination type. should this fail it throws an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="targetAssembly"></param>
        /// <param name="targetTypeName"></param>
        /// <returns>the converted value</returns>
        /// <exception cref="TypeNotFoundException"></exception>
        /// <exception cref="FailedToCastTypeException"></exception>
        /// <exception cref="CastTypeNotSupportedException"></exception>
        public static dynamic CastPrimitive<T>(dynamic from, T to, Assembly? targetAssembly = null, string? targetTypeName = null) where T : notnull =>
            CastPrimitive(from, FindType(to.GetType().Name, to.GetType().Assembly), targetAssembly, targetTypeName);
        /// <summary>
        /// Casts the given file to the given destination type. should this fail it throws an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <returns>the converted value</returns>
        /// <exception cref="TypeNotFoundException"></exception>
        /// <exception cref="FailedToCastTypeException"></exception>
        /// <exception cref="CastTypeNotSupportedException"></exception>
        public static dynamic CastPrimitive<T>(dynamic from) where T : notnull => CastPrimitive(from, FindType(typeof(T).Name, typeof(T).Assembly));
        /// <summary>
        /// Attemts to cast a primitive value to the given cast target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="result"></param>
        /// <returns>true if the operation was successful, otherwise false. puts the result in <paramref name="result"/>. if the operation failed this will be the default value of the specified target type</returns>
        public static bool TryCastPrimitive<T>(dynamic from, out T result) where T : notnull
        {
            try
            {
                result = CastPrimitive<T>(from);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
        /// <summary>
        /// Attemts to cast a primitive value to the given cast target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="result"></param>
        /// <returns>true if the operation was successful, otherwise false. puts the result in <paramref name="result"/>. 
        /// if the operation failed null is returned</returns>
        public static bool TryCastPrimitive(dynamic from, Type to, out dynamic result)
        {
            try
            {
                result = CastPrimitive(from, to);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }


    }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Gets thrown when casting fails
    /// </summary>
    [Serializable]
    public class FailedToCastTypeException : WinterException
    {

        public FailedToCastTypeException() { }
        public FailedToCastTypeException(string message) : base(message) { }
        public FailedToCastTypeException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Gets thrown when destination type is not supported by the <see cref="TypeWorker.CastPrimitive{T}(dynamic)"/> methods
    /// </summary>
    [Serializable]
    public class CastTypeNotSupportedException : WinterException
    {
        public CastTypeNotSupportedException() { }
        public CastTypeNotSupportedException(string message) : base(message) { }
        public CastTypeNotSupportedException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Gets thrown when a linked method for events when serializing is not found
    /// </summary>
    [Serializable]
    public class MethodNotFoundException : WinterException
    {
        public MethodNotFoundException() { }
        public MethodNotFoundException(string message) : base(message) { }
        public MethodNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
