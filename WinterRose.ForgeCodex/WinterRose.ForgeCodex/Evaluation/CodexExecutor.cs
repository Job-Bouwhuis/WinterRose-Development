using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.AnonymousTypes;
using WinterRose.ForgeCodex.Parsing.Ast;
using WinterRose.ForgeCodex.Parsing.Modifiers;

namespace WinterRose.ForgeCodex.Evaluation;

internal static class CodexExecutor
{
    public static object Evaluate(CodexDatabase db, QueryRoot root)
    {
        object? result = null;
        if (root.From is IEvaluable execFrom)
            result = execFrom.Evaluate(new EvaluationContext(db));

        if (root.Take != null)
        {
            if (result is not List<int> rows)
                throw new InvalidOperationException("take expects rows to be selected using 'from' and optionally 'where'");
            result = ExecuteTake(db, db.GetTable(root.From.SourceTypeName), root.Take, rows);
        }
        else
            return result;

        foreach (var modifier in root.Modifiers)
            ApplyModifier(result, modifier);

        return result;
    }

    private static IReadOnlyList<Anonymous> ExecuteTake(CodexDatabase db, Table root, QueryTake take, List<int> rows)
    {
        Table table = db.Traverse(root, take.Path);

        List<Anonymous> results = [];
        
        // Iterate each row in scope
        foreach (int rowIndex in rows)
        {
            EvaluationContext ctx = new(db, table, rowIndex);
            Anonymous rowObj = new();

            // Populate the row object with requested fields
            foreach (var entry in take.Selection.Entries)
            {
                if (entry.NestedSelection == null)
                {
                    // Simple field selection
                    var value = ctx.GetValue(entry.FieldName);
                    rowObj[entry.FieldName] = value;
                }
                else
                {
                    // TODO make join
                    rowObj[entry.FieldName] = "Joining not yet implemented";
                }
            }

            results.Add(rowObj);
        }

        return results;
    }
    private static void ApplyModifier(object? result, Modifier modifier)
    {

    }
}
