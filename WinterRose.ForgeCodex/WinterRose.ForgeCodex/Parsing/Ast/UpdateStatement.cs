using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public class UpdateStatement : QueryFrom
{
    public AssignmentBlock Values { get; set; }
    public bool ElseAdd { get; set; }

    public UpdateStatement(string sourceTypeName, Expression? where, AssignmentBlock values, bool elseAdd)
        : base(sourceTypeName, where)
    {
        Values = values;
        ElseAdd = elseAdd;
    }
}

