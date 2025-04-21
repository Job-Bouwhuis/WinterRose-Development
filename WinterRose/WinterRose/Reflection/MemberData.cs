using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Reflection
{
    [DebuggerDisplay("{ToDebuggerString()}")]
    public sealed class MemberData
    {
        FieldInfo? fieldsource;
        PropertyInfo? propertysource;

        public MemberData(FieldInfo field)
        {
            fieldsource = field;
            Attributes = field.GetCustomAttributes().ToArray();
        }
        public MemberData(PropertyInfo property)
        {
            propertysource = property;
            Attributes = property.GetCustomAttributes().ToArray();
        }

        public static implicit operator MemberData(FieldInfo field) => new(field);
        public static implicit operator MemberData(PropertyInfo property) => new(property);

        /// <summary>
        /// The identifier of the field or property.
        /// </summary>
        public string Name => fieldsource?.Name ?? propertysource?.Name ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// The kind of member this is.
        /// </summary>
        public MemberTypes MemberType => fieldsource is not null ? MemberTypes.Field : MemberTypes.Property;
        /// <summary>
        /// The type of the field or property.
        /// </summary>
        public Type Type => fieldsource?.FieldType ?? propertysource?.PropertyType ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// The custom attributes on the field or property.
        /// </summary>
        public Attribute[] Attributes { get; }
        /// <summary>
        /// Field attributes, if this is a field. Otherwise, throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        public FieldAttributes FieldAttributes => fieldsource?.Attributes ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// Property attributes, if this is a property. Otherwise, throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        public PropertyAttributes PropertyAttributes => propertysource?.Attributes ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// Indicates if the field or property is public.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                if (fieldsource != null)
                    return fieldsource.IsPublic;

                if (propertysource != null)
                {
                    MethodInfo? getMethod = propertysource.GetMethod;
                    if (getMethod != null)
                        return getMethod.IsPublic;
                }

                throw new InvalidOperationException("No field or property found.");
            }
        }
        /// <summary>
        /// Indicates if the property has a setter.
        /// </summary>
        public bool PropertyHasSetter => propertysource?.GetSetMethod(true) != null;
        /// <summary>
        /// Indicates if the field or property is static.
        /// </summary>
        public bool IsStatic
        {
            get
            {
                if (fieldsource != null)
                {
                    // Check if field is static
                    return fieldsource.IsStatic;
                }

                if (propertysource != null)
                {
                    // Check if the property getter method is static
                    var getMethod = propertysource.GetGetMethod(true);
                    if (getMethod != null)
                    {
                        return getMethod.IsStatic;
                    }
                }

                // If neither field nor property found, throw an exception
                throw new InvalidOperationException("No field or property found.");
            }
        }

        /// <summary>
        /// Indicates if the field is readonly. eg const or readonly
        /// </summary>
        public bool IsInitOnly => fieldsource?.IsInitOnly ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// Indicates if the field is a literal. eg const or static readonly
        /// </summary>
        public bool IsLiteral => fieldsource?.IsLiteral ?? throw new InvalidOperationException("No field or property found.");
        /// <summary>
        /// Indicates if the field or property can be written to.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (fieldsource is not null)
                {
                    return !fieldsource.IsInitOnly && !fieldsource.IsLiteral;
                }
                else if (propertysource is not null)
                {
                    return propertysource.CanWrite;
                }
                else
                {
                    throw new InvalidOperationException("No field or property found.");
                }
            }
        }
        /// <summary>
        /// Whether or not the type is a reference type
        /// </summary>
        public bool ByRef => fieldsource?.FieldType.IsByRef ?? propertysource?.PropertyType.IsByRef ?? throw new InvalidOperationException("No field or property found.");

        /// <summary>
        /// Whether or not there actually is a field or property to read/write to.
        /// </summary>
        public bool Exists => fieldsource != null || propertysource != null;

        /// <summary>
        /// Gets the value stored at this field or property
        /// </summary>
        /// <returns>The object stored in the field or property</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object? GetValue(object? obj)
        {
            if (propertysource is null && fieldsource is null)
                throw new InvalidOperationException("No property or field found.");

            if (IsStatic)
                return fieldsource?.GetValue(null)
                    ?? propertysource?.GetValue(null);

            if (MemberType is MemberTypes.Property)
                return propertysource?.GetValue(obj);

            if (ByRef)
            {
                TypedReference tr = __makeref(obj);
                return fieldsource?.GetValueDirect(tr)
                    ?? throw new InvalidOperationException("No field found.");
            }

            return fieldsource?.GetValue(obj);
        }
        /// <summary>
        /// Writes the value to the field or property. If the field or property is readonly, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetValue(ref object? obj, object? value)
        {
            if (fieldsource is not null)
                SetFieldValue(obj, value);
            else if (propertysource is not null)
                SetPropertyValue(obj, value);
            else
                throw new Exception("Field or property does not exist with name: " + Name);
        }

        public void SetPropertyValue<T>(object? obj, T value)
        {
            object actualValue = value;
            if (value != null)
            {
                MethodInfo? conversionMethod = TypeWorker.FindImplicitConversionMethod(Type, value.GetType());

                if (conversionMethod != null)
                    actualValue = conversionMethod.Invoke(null, new object[] { value })!;
            }

            if (obj is null && !(Type.IsAbstract && Type.IsSealed))
                throw new Exception("Reflection helper was created type only.");

            propertysource.SetValue(obj, actualValue);
        }

        public void SetFieldValue<T>(object obj, T value)
        {
            // Check if the field's type or the value type has a compatible implicit conversion operator
            MethodInfo? conversionMethod = TypeWorker.FindImplicitConversionMethod(Type, typeof(T));

            object? actualValue = value;

            if (conversionMethod != null)
            {
                // Convert the value using the implicit operator if it exists
                actualValue = conversionMethod.Invoke(null, [value]);
            }

            if (obj is null && !(Type.IsAbstract && Type.IsSealed))
                throw new Exception("Reflection helper was created type only.");

            if (!Type.IsByRef)
            {
                fieldsource!.SetValue(obj, actualValue);
            }
            else
            {
                fieldsource!.SetValueDirect(__makeref(obj), actualValue);
            }
        }

        /// <summary>
        /// Gets whether the field or property has the provided attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if the field or property has at least 1 attribute of the given type <typeparamref name="T"/></returns>
        public bool HasAttribute<T>()
        {
            foreach (var attr in Attributes)
            {
                if (attr is T)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the attribute of the specified type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The first found attribute of type <typeparamref name="T"/>. if there is no such attribute, <c>null</c> is returned</returns>
        public T? GetAttribute<T>() where T : Attribute
        {
            foreach (var attr in Attributes)
            {
                if (attr is T a)
                    return a;
            }
            return null;
        }

        private string ToDebuggerString()
        {
            string publicOrPrivate = IsPublic ? "Public" : "Private";
            string writable = CanWrite ? "Writable" : "Readonly";
            string propOrField = MemberType == MemberTypes.Field ? "Field" : "Property";
            return $"{publicOrPrivate} {propOrField} <{{{Name}}} = {writable}";
        }
    }
}
