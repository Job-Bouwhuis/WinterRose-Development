using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

/// <summary>
/// Fields of properties with this attribute do not get shown in the <see cref="Hirarchy"/> or <see cref="EditorMode.Editor"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class HideAttribute : Attribute
{
}
