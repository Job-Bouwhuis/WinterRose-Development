using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Evaluation;

public sealed class EvaluationContext
{
    public CodexDatabase Database { get; }
    public Table? CurrentTable { get; }
    public int? CurrentRow { get; }

    public EvaluationContext(CodexDatabase db, Table? table = null, int? row = null)
    {
        Database = db;
        CurrentTable = table;
        CurrentRow = row;
    }

    public object? GetValue(string columnName)
    {
        if (CurrentTable == null || CurrentRow == null)
            throw new InvalidOperationException("Value lookup requires a table and row.");

        var col = CurrentTable.GetColumn(columnName);
        return col[CurrentRow.Value].Value;
    }
}
