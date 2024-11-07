using System;
namespace WinterRose.Monogame;

/// <summary>
/// The <see cref="Monogame.Worlds.WorldTemplateCreator"/> should ignore this field
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
public class IgnoreInTemplateCreationAttribute : Attribute
{
}
