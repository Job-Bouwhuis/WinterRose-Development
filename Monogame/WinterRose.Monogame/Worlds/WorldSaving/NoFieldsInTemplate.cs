using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

/// <summary>
/// The <see cref="NoFieldsInTemplate"/> attribute is used to mark a class as not having any fields in the template."/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NoFieldsInTemplate : Attribute
{

}
