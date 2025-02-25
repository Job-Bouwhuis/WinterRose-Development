using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose
{
    /// <summary>
    /// Provides extra methods for creating instances of types. Works in conjunction with the <see cref="DefaultArgumentsAttribute"/>.
    /// </summary>
    public static class ActivatorExtra
    {
        /// <summary>
        /// Creates an instance of the given generic type. The type must have a parameterless constructor, 
        /// or a constructor with the <see cref="DefaultArgumentsAttribute"/>.
        /// </summary>
        /// <returns>The created object. if the type has no parameterless constructor, 
        /// and no constructor with the <see cref="DefaultArgumentsAttribute"/>, returns null.</returns>
        public static object? CreateInstance(Type type, params object[] args)
        {
            if (args.Length is not 0)
            {
                Type[] types = new Type[args.Length];
                args.Foreach((x, i) => types[i] = x.GetType());

                ConstructorInfo? info = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null) ??
                    throw new NullReferenceException("No constructor with provided argument types was found.");
                return info.Invoke(args);
            }


            //Get all constructors
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (ConstructorInfo constructor in constructors)
            {
                //fetch all attributes that are of type DefaultArgumentAttribute
                object[] attributes = constructor.GetCustomAttributes(typeof(DefaultArgumentsAttribute), false);
                if (attributes.Any(x => x is DefaultArgumentsAttribute))
                {
                    DefaultArgumentsAttribute attribute = attributes.First(x => x is DefaultArgumentsAttribute) as DefaultArgumentsAttribute;
                    return constructor.Invoke(attribute.Arguments);
                }
            }

            try
            {
                ConstructorInfo creator = constructors.OrderBy(x => x.GetParameters().Length).First();
                return creator.Invoke(null);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates an instance of the given generic type. The type must have a parameterless constructor, 
        /// or a constructor with the <see cref="DefaultArgumentsAttribute"/>.
        /// </summary>
        /// <returns>The created object. if the type has no parameterless constructor, 
        /// and no constructor with the <see cref="DefaultArgumentsAttribute"/>, returns null.</returns>
        public static T? CreateInstance<T>(params object[] args) => (T?)CreateInstance(typeof(T), args);

        /// <summary>
        /// Creates an instance of the given generic type. The type must have a parameterless constructor, 
        /// or a constructor with the <see cref="DefaultArgumentsAttribute"/>.<br></br>
        /// With <paramref name="constructorIndex"/> you can specify which constructor to use out of the valid constructors. <b>the constructors are sorted on their position in the script</b>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorIndex">Index is zero-based.</param>
        /// <returns>The created object. if the type has no parameterless constructor, 
        /// and no constructor with the <see cref="DefaultArgumentsAttribute"/>, returns null.</returns>
        public static object? CreateInstance(Type type, int constructorIndex)
        {
            //Get all constructors
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            //Get all constructors with the DefaultArgumentsAttribute
            List<ConstructorInfo> validConstructors =
                constructors.Where(x => x.GetCustomAttributes(typeof(DefaultArgumentsAttribute), false).Any()).ToList();

            //Get the empty constructor, if it exists
            ConstructorInfo? parameterless = constructors.Where(x => x.GetParameters().Length == 0).FirstOrDefault();

            //add it to the list if it exists
            if (parameterless is not null)
                validConstructors = validConstructors.Prepend(parameterless).ToList();

            //if the index is out of range, throw an error
            if (constructorIndex >= validConstructors.Count)
                throw new ArgumentOutOfRangeException($"{type.Name} does not have a valid constructor at index {constructorIndex}. Valid constructors: {validConstructors.Count}");

            // Fetch the constructor at the given index
            ConstructorInfo constructor = validConstructors[constructorIndex];

            try
            {
                //if the selected constructor has a DefaultArgumentsAttribute, use that
                if (constructor.GetCustomAttributes(typeof(DefaultArgumentsAttribute), false).Any())
                {
                    //get the attribute
                    DefaultArgumentsAttribute attribute = constructor.GetCustomAttributes(typeof(DefaultArgumentsAttribute), false).First() as DefaultArgumentsAttribute;
                    //invoke the constructor with the arguments from the attribute, and return the result
                    return constructor.Invoke(attribute.Arguments);
                }
                //else use it as a empty constructor
                else
                {
                    //return the result of invoking the empty constructor
                    return constructor.Invoke(null);
                }
            }
            catch (TargetInvocationException)
            {
                //the TargetInvocationException should not be ignored. It is thrown when the constructor throws an exception.
                throw;
            }
            catch
            {
                // if it throws any other exception it returns null
                return null;
            }
        }

        /// <summary>
        /// Creates an instance of the given generic type. The type must have a parameterless constructor, 
        /// or a constructor with the <see cref="DefaultArgumentsAttribute"/>.<br></br>
        /// With <paramref name="constructorIndex"/> you can specify which constructor to use out of the valid constructors. <b>the constructors are sorted on their position in the script</b>
        /// </summary>
        /// <param name="constructorIndex">Index is zero-based.</param>
        /// <returns>The created object. If </returns>
        public static T? CreateInstance<T>(int constructorIndex) => (T?)CreateInstance(typeof(T), constructorIndex);

        /// <summary>
        /// Creates an instance of the type who's type and assembly names are given. optionally filters on namespaces aswell.
        /// </summary>
        /// <param name="typeName">the name of the type</param>
        /// <param name="assemblyName">the name of the assembly</param>
        /// <param name="namespaceName">the namespace</param>
        /// <returns>An instance of the found type. or null if noe type was found.</returns>
        public static object? CreateInstanceOf(string typeName, string assemblyName, string? namespaceName = null)
        {
            Type? t;
            if (namespaceName is not null)
                t = TypeWorker.FindType(typeName, assemblyName, namespaceName);
            else
                t = TypeWorker.FindType(typeName, assemblyName);

            if (t is null)
                throw new TypeNotFoundException($"Type with name {typeName} was not found in Assembly {assemblyName}" +
                                                namespaceName is null ? "." : $" and in the namespace {namespaceName}");

            return CreateInstance(t);
        }
    }
}


