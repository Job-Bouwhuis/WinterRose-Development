using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

/// <summary>
/// Fields or properties with this attribute that are not public will be shown in the <see cref="Hirarchy"/> or <see cref="EditorMode.Editor"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ShowAttribute : Attribute
{
}
