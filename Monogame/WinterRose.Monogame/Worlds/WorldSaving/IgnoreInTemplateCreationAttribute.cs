using System;
namespace WinterRose.Monogame;

/// <summary>
/// The <see cref="Monogame.Worlds.WorldTemplateCreator"/> should ignore this field
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class IgnoreInTemplateCreationAttribute : Attribute
{
}
