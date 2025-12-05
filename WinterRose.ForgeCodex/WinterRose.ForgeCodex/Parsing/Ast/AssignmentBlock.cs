namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class AssignmentBlock
{
    public List<AssignmentEntry> Entries { get; }
    public AssignmentBlock(List<AssignmentEntry> entries) => Entries = entries;
}
