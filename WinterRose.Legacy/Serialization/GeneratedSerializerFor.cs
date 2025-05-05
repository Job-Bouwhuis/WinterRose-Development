using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Legacy.Serialization;

/// <summary>
/// Dictates the class is a generated serialzier for <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class GeneratedSerializerFor<T> : Attribute
{
}
