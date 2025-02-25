using Microsoft.Xna.Framework;
using SharpDX.Direct2D1;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// Used to create world saves that can be loaded again when loading a <see cref="World"/>
/// </summary>
public sealed class WorldTemplateCreator
{
    private World world;
    private string path;
    private WorldTemplateTypeSearchOverrideCollection typeOverrides = new();

    /// <summary>
    /// Creates a new <see cref="WorldTemplateCreator"/> with the given <paramref name="templateDestinationPath"/> and <paramref name="world"/>
    /// </summary>
    /// <param name="templateDestinationPath">The path where the saved world should be stored at</param>
    /// <param name="world">The world to work with</param>
    /// <param name="typeOverrides">The type overrides to use when creating this template</param>
    public WorldTemplateCreator(string templateDestinationPath, World world, params WorldTemplateTypeSearchOverride[] typeOverrides)
    {
        path = templateDestinationPath;
        this.world = world;
        this.typeOverrides.AddRange(typeOverrides);
    }
    /// <summary>
    /// Creates a new <see cref="WorldTemplateCreator"/> having only the type overrides given. creates a new <see cref="World"/> to work with
    /// </summary>
    /// <param name="typeOverrides"></param>
    public WorldTemplateCreator(params WorldTemplateTypeSearchOverride[] typeOverrides) : this("New World Template", new World(), typeOverrides) { }
    /// <summary>
    /// Creates a new save at the given <paramref name="templateDestinationPath"/> with the given <paramref name="world"/> and <paramref name="typeOverrides"/>
    /// </summary>
    /// <param name="templateDestinationPath">The path where the saved world should be stored at</param>
    /// <param name="world">The world to work with</param>
    /// <param name="typeOverrides">The type overrides to use when creating this template</param>
    public static void CreateSave(string templateDestinationPath, World world, params WorldTemplateTypeSearchOverride[] typeOverrides)
    {
        new WorldTemplateCreator(templateDestinationPath, world, typeOverrides).CreateSave();
    }
    /// <summary>
    /// Creates the save of the world
    /// </summary>
    public void CreateSave()
    {
        StringBuilder objectDefs = new();
        foreach (var obj in world.Objects)
        {
            objectDefs.Append(GetObjectDefinition(obj));
        }
        StringBuilder result = new();
        foreach (var typeOverride in typeOverrides)
            result.AppendLine($"{typeOverride.Identifier} = {typeOverride.Type.FullName}");

        result.AppendLine("\n\n\n");

        result.Append(objectDefs);

        FileManagement.FileManager.Write(path, result, true);
    }

    private StringBuilder GetObjectDefinition(WorldObject obj)
    {
        StringBuilder result = new();

        result.AppendLine($"object {obj.Name}:");
        foreach (ObjectComponent comp in obj.FetchComponents())
        {
            if (comp is Transform)
                continue;
            result.Append(GetComponentDefinition(comp));
        }
        result.Append(ParseTransform(obj));

        result.AppendLine($"end {obj.Name}\n");
        return result;
    }
    private StringBuilder ParseTransform(WorldObject obj)
    {
        StringBuilder result = new();
        WorldTemplateTypeSearchOverride vecType = new(typeof(Vector2), typeof(Vector2).Name);
        if (!typeOverrides.Any(vecType))
            typeOverrides.Add(vecType);
        WorldTemplateTypeSearchOverride floatType = new(typeof(float), typeof(float).Name);
        if (!typeOverrides.Any(floatType))
            typeOverrides.Add(floatType);

        if (obj.transform.position != Vector2.Zero)
            result.AppendLine($"\ttransform.position = {vecType.GetParsedString(obj.transform.position)}");
        if (obj.transform.scale != Vector2.One)
            result.AppendLine($"\ttransform.scale = {vecType.GetParsedString(obj.transform.scale)}");
        if (obj.transform.rotation != 0f)
            result.AppendLine($"\ttransform.rotation = {floatType.GetParsedString(obj.transform.rotation)}");
        if (obj.transform.parent is not null)
            result.AppendLine($"\ttransform.parent = {obj.transform.parent.owner.Name}.transform");
        if (obj.Flag is not "")
            result.AppendLine($"\tFlag = \"{obj.Flag}\"");
        return result;
    }
    private StringBuilder GetComponentDefinition(ObjectComponent component)
    {
        StringBuilder result = new();
        Type type = component.GetType();
        WorldTemplateTypeSearchOverride typeOverride = new(type, type.Name);
        if (!typeOverrides.Any(typeOverride))
            typeOverrides.Add(typeOverride);
        string name = type.Name;

        string? def = typeOverrides.GetDefinition(type, component);
        if (def is null)
            result.AppendLine($"\t{name}");
        else
            result.AppendLine($"\t{def}");

        if (type.GetCustomAttributes<NoFieldsInTemplate>().Any())
            return result;

        result.Append(ParseFields(name, type, component));
        result.Append(ParseProperties(name, type, component));

        return result;
    }
    private StringBuilder ParseFields(string name, Type type, ObjectComponent component)
    {
        StringBuilder result = new();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.Name.Contains('<') || field.Name is "_owner")
                continue;
            if (field.GetCustomAttributes<IgnoreInTemplateCreationAttribute>().Any())
                continue;

            object? value = field.GetValue(component);
            if (value is null)
                continue;
            if (component.owner.HasComponent(value.GetType()))
                value = value.GetType().Name;

            if (value is null)
                continue;

            WorldTemplateTypeSearchOverride typeOverrideDefinition = new(field.FieldType, field.FieldType.Name);
            if (!typeOverrides.Any(typeOverrideDefinition))
                typeOverrides.Add(typeOverrideDefinition);

            value = typeOverrideDefinition.GetParsedString(value);
            result.AppendLine($"\t{name}.{field.Name} = {value}");
        }
        return result;
    }
    private StringBuilder ParseProperties(string name, Type type, ObjectComponent component)
    {
        StringBuilder result = new();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!property.GetCustomAttributes<IncludeInTemplateCreationAttribute>().Any())
                continue;

            object? value = property.GetValue(component);

            if (value is null)
                continue;

            WorldTemplateTypeSearchOverride typeOverrideDefinition = new(property.PropertyType, property.PropertyType.Name.Replace('`', '_'));
            if (property.PropertyType.IsGenericType && !typeOverrideDefinition.HasCustomParser)
                throw new InvalidOperationException("Can not use generic types in template creation that do not have special parsing. See WorldTemplateObjectParsers class to create one!");

            if (!typeOverrides.Any(typeOverrideDefinition) && !property.PropertyType.IsGenericType)
                typeOverrides.Add(typeOverrideDefinition);

            value = typeOverrideDefinition.GetParsedString(value);
            result.AppendLine($"\t{name}.{property.Name} = {value}");
        }
        return result;
    }

    internal string CreateSaveOf(WorldObject obj)
    {
        var objDef = GetObjectDefinition(obj);

        StringBuilder result = new();
        foreach (var typeOverride in typeOverrides)
            result.AppendLine($"{typeOverride.Identifier} = {typeOverride.Type.FullName}");

        result.AppendLine("\n\n\n");

        result.Append(objDef);

        return result.ToString();
    }
}