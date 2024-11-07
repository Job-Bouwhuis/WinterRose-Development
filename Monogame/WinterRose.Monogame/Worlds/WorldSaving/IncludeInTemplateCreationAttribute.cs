using System;
using System.Collections.Generic;
namespace WinterRose.Monogame;

/// <summary>
/// The <see cref="Worlds.WorldTemplateCreator"/> should include this property
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class IncludeInTemplateCreationAttribute : Attribute
{
    /// <summary>
    /// The condition that must be met for this property to be included in the template. By default, this is always true.
    /// </summary>
    public Func<object, bool> Condition { get; set; } = (obj) => true; 
}