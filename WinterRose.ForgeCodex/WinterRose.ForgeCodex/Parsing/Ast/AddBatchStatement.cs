namespace WinterRose.ForgeCodex.Parsing.Ast;

public class AddBatchStatement : AddStatement
{
    public string TargetName { get; set; }
    public List<AssignmentBlock> Blocks { get; set; }

    public AddBatchStatement(string? sourceTypeName, Expression? where, string targetName, List<AssignmentBlock> blocks)
        : base(sourceTypeName, where, targetName, null)
    {
        TargetName = targetName;
        Blocks = blocks;
    }
}