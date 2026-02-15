namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// RichText element that invokes a registered function during rendering.
/// </summary>
public class RichFunction : RichElement
{
    public string FunctionName { get; set; }
    public Dictionary<string, string> Arguments { get; set; } = new();

    public RichFunction(string functionName)
    {
        FunctionName = functionName;
    }

    public RichFunction(string functionName, Dictionary<string, string> arguments)
    {
        FunctionName = functionName;
        Arguments = arguments ?? new();
    }

    public override string ToString() => $"\\function[{FunctionName}]";

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        // Check if function registry is available in context
        if (!context.AdditionalData.TryGetValue("FunctionRegistry", out var registryObj) 
            || registryObj is not FunctionRegistry registry)
        {
            // No function registry available - render nothing
            return new RichTextRenderResult
            {
                WidthConsumed = 0,
                HeightConsumed = 0
            };
        }

        // Invoke the function
        var result = registry.InvokeFunction(FunctionName, Arguments, context, position);

        return new RichTextRenderResult
        {
            WidthConsumed = result.WidthConsumed,
            HeightConsumed = result.HeightConsumed
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        // Functions don't consume width during measurement
        return 0;
    }
}
