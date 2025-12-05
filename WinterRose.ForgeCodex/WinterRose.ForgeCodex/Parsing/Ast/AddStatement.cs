using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public class AddStatement : QueryFrom
{
    public string TargetName { get; }
    public AssignmentBlock Values { get; }

    public AddStatement(string sourceTypeName, Expression? where, string targetName, AssignmentBlock values)
        : base(sourceTypeName, where)
    {
        TargetName = targetName;
        Values = values;
    }

    public override object? Evaluate(EvaluationContext context)
    {
        var db = context.Database;

        if (!db.HasTable(TargetName))
            throw new InvalidOperationException($"Table '{TargetName}' does not exist.");

        var table = db.GetTable(TargetName);

        var rowData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in Values.Entries)
        {
            var value = entry.Value.Evaluate(new EvaluationContext(db, table));
            rowData[entry.Field] = value;
        }

        table.AddRow(rowData);

        db.StorageProvider?.WriteTable(table);

        return null;
    }
}

