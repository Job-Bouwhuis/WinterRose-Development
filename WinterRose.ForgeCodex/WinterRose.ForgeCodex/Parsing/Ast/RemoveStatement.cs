using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class RemoveStatement : QueryFrom
{
    public RemoveStatement(string targetName, Expression deletehere)
        : base(targetName, deletehere)
    {
    }

    public override object? Evaluate(EvaluationContext context)
    {
        List<int> rowsToDelete = (List<int>)base.Evaluate(context);

        Table table = context.Database.GetTable(SourceTypeName);
        foreach (int row in rowsToDelete)
            table.RemoveRow(row);

        context.Database.StorageProvider?.WriteTable(table);
        return rowsToDelete.Count > 0;
    }
}

