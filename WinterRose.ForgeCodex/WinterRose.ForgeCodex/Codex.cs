using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.Parsing;
using WinterRose.ForgeCodex.Parsing.Ast;

namespace WinterRose.ForgeCodex;

public static class Codex
{
    public static QueryRoot Parse(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query string is empty");

        var result = new Parser(query).Parse();
        return result;
    }

}
