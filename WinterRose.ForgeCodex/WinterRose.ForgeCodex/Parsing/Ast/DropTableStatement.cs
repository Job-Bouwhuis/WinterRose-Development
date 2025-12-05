using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class DropTableStatement : QueryFrom
{
    public DropTableStatement(string tableName)
        : base(tableName, null)
    {
    }

    public override object? Evaluate(EvaluationContext context)
    {
        if (!context.Database.HasTable(SourceTypeName))
            throw new InvalidOperationException($"Table {SourceTypeName} does not exist in the database");
        context.Database.RemoveTable(SourceTypeName);
        return true;
    }
}

