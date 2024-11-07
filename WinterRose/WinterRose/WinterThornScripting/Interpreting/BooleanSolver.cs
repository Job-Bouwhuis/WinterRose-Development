using System;
using System.Collections.Generic;
using System.Linq;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Expressions;

/// <summary>
/// Solves boolean expressions
/// </summary>
public static class BooleanSolver
{
    /// <summary>
    /// A list of all the boolean operators supported.
    /// </summary>
    public static List<string> BooleanOperators { get; } = new() { "==", "!=", ">", "<", ">=", "<=", "&&", "||", "^" };

    private static bool fromInterpreter = true;
    private static Block userDefinedVariables = new(null);

    /// <summary>
    /// Solves a boolean expression.
    /// </summary>
    /// <param name="toSolve"></param>
    /// <returns></returns>
    public static bool Solve(string toSolve)
    {
        List<Token> tokens = Tokenizer.Tokenize(toSolve);

        Block b = new(null);
        foreach (var var in userDefinedVariables.Variables)
        {
            b.DeclareVariable(var);
        }

        fromInterpreter = false;
        bool res = (bool)Solve(tokens, ref b).Value!;
        fromInterpreter = true;
        return res;
    }

    internal static Variable Solve(List<Token> tokens, ref Block block)
    {
        // Handle parentheses and nested expressions first
        tokens = SolveParentheses(tokens, ref block);

        tokens = SolveMathExpressionsBeforeBoolean(tokens, ref block);

        List<Operation> parts = new();
        List<OpResult> results = new();

        if (tokens.Count == 1)
        {
            var var = block[tokens[0].Identifier] ?? throw new booleanSolverException(tokens);
            return (bool)var.Value!;
        }

        for (int i = 0; i < tokens.Count; i += 3)
        {
            Token? left = tokens[i];
            Token? op = tokens[i + 1];
            Token? right = tokens[i + 2];
            Token? nextOp = null;
            if (tokens.Count > i + 3)
            {
                nextOp = tokens[i + 3];
                i++;
            }

            parts.Add(new(block[left.Identifier], op.Identifier, block[right.Identifier], nextOp?.Identifier));
        }

        for (int i = 0; i < parts.Count; i++)
        {
            Operation part = parts[i];
            bool res = Solve(part.Left, part.Operator, part.Right);
            results.Add(new(res, part.opNext));
        }

        if (results.Count == 0)
        {
            if (fromInterpreter)
                throw new WinterThornExecutionError(ThornError.ExpressionError, "WT-0013", "Invalid boolean expression.");
            else
                throw new Exception("Invalid boolean expression");
        }

        // Handle chaining
        bool finalResult = results[0].result;

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].opNext == null)
                break;

            finalResult = Solve(finalResult, results[i].opNext, results[i + 1].result);
        }

        return finalResult;
    }

    // New method to handle parentheses and operator precedence
    private static List<Token> SolveParentheses(List<Token> tokens, ref Block block)
    {
        Stack<int> openParenIndices = new();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Identifier == "(")
            {
                openParenIndices.Push(i);
            }
            else if (tokens[i].Identifier == ")")
            {
                if (openParenIndices.Count == 0)
                {
                    throw new Exception("Mismatched parentheses.");
                }

                int openIndex = openParenIndices.Pop();
                var subExpressionTokens = tokens.GetRange(openIndex + 1, i - openIndex - 1);

                // Solve the expression inside the parentheses
                Variable result = Solve(subExpressionTokens, ref block);

                // Replace the entire sub-expression (including parentheses) with the result token
                tokens.RemoveRange(openIndex, i - openIndex + 1);
                tokens.Insert(openIndex, new Token(result.Value.ToString().ToLower(), TokenType.Identifier)); // Assume result is a boolean value
                i = openIndex; // Reset index to recheck the simplified expression
            }
        }

        if (openParenIndices.Count > 0)
        {
            throw new Exception("Mismatched parentheses.");
        }

        return tokens;
    }

    private static List<Token> SolveMathExpressionsBeforeBoolean(List<Token> tokens, ref Block block)
    {
        List<Token> newTokens = new List<Token>();

        // Temporary storage for a possible math expression
        List<Token> mathTokens = new List<Token>();

        foreach (var token in tokens)
        {
            if (IsMathOperator(token) || IsNumber(token, ref block))
            {
                // Accumulate math tokens
                mathTokens.Add(token);
            }
            else if (IsBooleanOperator(token))
            {
                // Solve the math expression if we have any math tokens collected
                if (mathTokens.Count > 0)
                {
                    if (mathTokens.Count is 1) // its just an identifier. calling the entire math solver for this would be too much overhead
                    {
                        newTokens.Add(mathTokens[0]);
                        mathTokens.Clear(); // Clear after solving
                    }
                    else
                    {
                        double mathResult = SolveMathExpression(mathTokens, ref block);
                        // Add the result of the math expression as a token
                        newTokens.Add(new Token(mathResult.ToString(), TokenType.Number));
                        mathTokens.Clear(); // Clear after solving
                    }
                }
                // Add the boolean operator to the new token list
                newTokens.Add(token);
            }
            else
            {
                // If it's not part of a math or boolean expression, add the token as is
                newTokens.Add(token);
            }
        }

        // Handle any remaining math tokens
        if (mathTokens.Count > 0)
        {
            if (mathTokens.Count is 1)
            {
                newTokens.Add(mathTokens[0]);
                return newTokens;
            }
            double mathResult = SolveMathExpression(mathTokens, ref block);
            newTokens.Add(new Token(mathResult.ToString(), TokenType.Number));
        }

        return newTokens;
    }

    private record Operation(Variable Left, string Operator, Variable Right, string? opNext);
    private record OpResult(bool result, string? opNext);

    private static bool Solve(Variable left, string op, Variable right)
    {
        if (left is null && right is null)
            return true;
        if (left is null && right is not null
            || right is null && left is not null
            || left.Value is null && right.Value is not null
            || right.Value is null && left.Value is not null)
            return false;



        switch (op)
        {
            case "==":
                if (left.Value == null && right.Value == null)
                    return true;

                return left.Value.Equals(right.Value);
            case "!=":
                if (left.Value == null && right.Value == null)
                    return false;

                return !left.Value.Equals(right.Value);
            case ">":
                return (double)left.Value > (double)right.Value;
            case "<":
                return (double)left.Value < (double)right.Value;
            case ">=":
                return (double)left.Value >= (double)right.Value;
            case "<=":
                return (double)left.Value <= (double)right.Value;
            case "&&":
                return (bool)left.Value && (bool)right.Value;
            case "||":
                return (bool)left.Value || (bool)right.Value;
            case "^":
                return (bool)left.Value ^ (bool)right.Value;
            default:
                if (!fromInterpreter)
                    throw new Exception("Fault at evaluating boolean expression.");
                throw new WinterThornExecutionError(ThornError.InterpreterError, "WSI-0003", "Fault at fetching Boolean operation tokens");
        }
    }

    /// <summary>
    /// Define a boolean variable to use in expressions
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Define(string name, bool value)
    {
        Variable var = new(name, "", AccessControl.Public);
        var.Value = value;
        userDefinedVariables.DeclareVariable(var);
    }

    /// <summary>
    /// Define a numerical variable to use in expressions
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Define(string name, double value)
    {
        Variable var = new(name, "", AccessControl.Public);
        var.Value = value;
        userDefinedVariables.DeclareVariable(var);
    }

    public static void Undefine(string name)
    {
        var toDelete = userDefinedVariables.Variables.FirstOrDefault(x => x.Name == name);
        if (toDelete != null)
            userDefinedVariables.Variables.Remove(toDelete);
    }

    private static bool IsMathOperator(Token token)
    {
        // Assuming your token type has math operators like "+", "-", "*", "/"
        return token.Type == TokenType.Operator
            && (token.Identifier == "+"
            || token.Identifier == "-"
            || token.Identifier == "*"
            || token.Identifier == "/"
            || token.Identifier == "%"
            || token.Identifier == "("
            || token.Identifier == ")");
    }

    private static bool IsNumber(Token token, ref Block block)
    {
        Variable? var = block[token.Identifier];
        if (var == null) return false;
        return var.Type == VariableType.Number;
    }

    private static bool IsBooleanOperator(Token token)
    {
        return BooleanOperators.Contains(token.Identifier);
    }

    private static double SolveMathExpression(List<Token> mathTokens, ref Block block)
    {
        return (double)MathSolver.Solve(mathTokens, ref block).Value!;
    }
}
