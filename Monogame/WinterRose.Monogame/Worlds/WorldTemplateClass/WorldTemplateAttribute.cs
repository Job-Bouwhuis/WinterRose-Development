using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// Specifies that the class is a world template.<br></br>
/// This class should inherit from <see cref="WorldTemplate"/> 
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class WorldTemplateAttribute : Attribute
{
}