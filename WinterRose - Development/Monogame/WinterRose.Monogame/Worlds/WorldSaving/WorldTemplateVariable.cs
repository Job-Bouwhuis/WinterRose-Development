using System.Diagnostics;

namespace WinterRose.Monogame.Worlds.WorldSaving;


[DebuggerDisplay("{Identifyer} = {Value}")]
internal class WorldTemplateVariable
{
    public string Identifyer;
    public string Value;

    public WorldTemplateVariable() { }
    public WorldTemplateVariable(string identifyer, string value)
    {
        Identifyer = identifyer;
        Value = value;
    }
}
