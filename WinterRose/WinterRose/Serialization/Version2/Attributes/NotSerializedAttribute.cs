using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose;

/// <summary>
/// Indicates that a field or property should not be serialized by <see cref="Serialization.Version2.SnowSerializer"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class NotSerializedAttribute : Attribute
{
}
