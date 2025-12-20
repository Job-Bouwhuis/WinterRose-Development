using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Interpreting;
using WinterRose.WIP.TestClasses;

namespace WinterRose.Expressions;

/// <summary>
/// Provides 
/// </summary>
public static class MathSolver
{
    public class EquationResult
    {
        public double Value { get; }

        public EquationResult(double value)
        {
            Value = value;
        }

        // Add any additional methods or properties as needed for your EquationResult class
    }

    private static Block userDefinedVariables = new(null);
    private static bool fromInterpreter = true;

    /// <summary>
    /// A list of supported math operators and their mathmatical priority
    /// </summary>
    public static Dictionary<string, int> OperatorPrecedence { get; } = new()
    {
        {"(", 4},
        {")", 4},
        {"*", 3},
        {"/", 3},
        {"%", 3},
        {"+", 2},
        {"-", 2},
        {"=", 1},
    };

    /// <summary>
    /// Solves the given mathmatical equation and returns the result
    /// </summary>
    /// <param name="equation"></param>
    /// <returns></returns>
    public static double Solve(string equation)
    {
        fromInterpreter = false;
        Block block = new(null);
        foreach (var var in userDefinedVariables.Variables)
            block.DeclareVariable(var);

        var val = Solve(Tokenizer.Tokenize(equation), ref block);
        fromInterpreter = true;
        return (double)val.Value!;
    }


    internal static Variable Solve(List<Token> tokens, ref Block block)
    {
        bool onlyMathOperators = tokens.Where(x => x.Type == TokenType.Operator).All(x => OperatorPrecedence.Any(op => op.Key == x.Identifier));
        
        if (onlyMathOperators)
        {
            tokens = GetPrioritizedFinalExpression(tokens, ref block);

            var res = ParseAndEvaluate(tokens, block);
            //SolveMathEquation(ref tokens, ref tokenIndex, ref workingValue, ref block);
            return new("Equation Result", $"The result of {GetString(tokens)}", res.Value, AccessControl.Private);
        }
        if (fromInterpreter)
            throw new WinterThornExecutionError(ThornError.InterpreterError, "WSI-0001", "Fault at fetching equation tokens");
        else
            throw new MathSolverException(tokens);
    }

    private static List<Token> GetPrioritizedFinalExpression(List<Token> tokens, ref Block block)
    {
        List<Token> finalTokens = new();
        Stack<int> parenthesesStack = new(); // Stack to track parentheses
        List<Token> currentExpressionTokens = new(); // Tokens for the current expression

        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];

            if (token.Type == TokenType.OpenParenthesis)
            {
                // Push the current index onto the stack
                parenthesesStack.Push(i);
            }
            else if (token.Type == TokenType.CloseParenthesis)
            {
                if (parenthesesStack.Count == 0)
                    throw new InvalidOperationException("Mismatched parentheses detected.");

                int startIndex = parenthesesStack.Pop();
                if (parenthesesStack.Count == 0) 
                {
                    var subExpressionTokens = tokens.Skip(startIndex + 1).Take(i - startIndex - 1).ToList();
                    double result = (double)Solve(subExpressionTokens, ref block).Value!; // Call the Solve method

                    finalTokens.Add(new Token(result.ToString(), TokenType.Number));
                }
            }
            else if (parenthesesStack.Count == 0)
            {
                // If not within parentheses, add the token to the final list
                finalTokens.Add(token);
            }
        }

        // If there are unmatched parentheses left
        if (parenthesesStack.Count > 0)
            throw new InvalidOperationException("Mismatched parentheses detected.");

        return finalTokens;
    }


    private static string GetString(List<Token> tokens)
    {
        StringBuilder sb = new();
        tokens.Foreach(x => sb.Append(x.Identifier).Append(' '));
        return sb.ToString();
    }

    private static EquationResult ParseAndEvaluate(List<Token> tokens, Block block)
    {
        var rootNode = BuildExpressionTree(tokens, block);
        return EvaluateNode(rootNode, block);
    }
    private static EquationResult EvaluateNode(Node node, Block block)
    {
        if (node is OperandNode operandNode)
        {
            return operandNode.Value; // Return the value of the operand node
        }
        else if (node is OperatorNode operatorNode)
        {
            EquationResult leftResult = EvaluateNode(operatorNode.Left, block);
            EquationResult rightResult = EvaluateNode(operatorNode.Right, block);

            return ApplyOperator(leftResult, rightResult, operatorNode.Operator);
        }
        else
        {
            throw new InvalidOperationException("Invalid node type encountered.");
        }
    }
    private static EquationResult ApplyOperator(EquationResult left, EquationResult right, Token op)
    {
        switch (op.Identifier)
        {
            case "+":
                return new EquationResult(left.Value + right.Value);
            case "-":
                return new EquationResult(left.Value - right.Value);
            case "*":
                return new EquationResult(left.Value * right.Value);
            case "/":
                if (right.Value == 0)
                {
                    return new(0);
                }
                return new EquationResult(left.Value / right.Value);
            case "%":
                return new EquationResult(left.Value % right.Value);
            default:
                throw new InvalidOperationException("Invalid operator encountered.");
        }
    }
    private static Node BuildExpressionTree(List<Token> tokens, Block block)
    {
        Stack<Node> nodeStack = new();
        Stack<Token> opStack = new();

        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];

            if (token.Type == TokenType.Number || token.Type == TokenType.Identifier)
            {
                nodeStack.Push(new OperandNode(new EquationResult((double)block[token.Identifier].Value)));
            }
            else if (token.Type == TokenType.Operator)
            {
                while (opStack.Any() && OperatorPrecedence[opStack.Peek().Identifier] >= OperatorPrecedence[token.Identifier])
                {
                    Token prevOp = opStack.Pop();
                    Node right = nodeStack.Pop();
                    Node left = nodeStack.Pop();
                    nodeStack.Push(new OperatorNode(left, right, prevOp));
                }
                opStack.Push(token);
            }
            else if (token.Type == TokenType.OpenParenthesis) // Handling '('
            {
                opStack.Push(token); // Push the '(' onto the operator stack
            }
            else if (token.Type == TokenType.CloseParenthesis) // Handling ')'
            {
                while (opStack.Any() && opStack.Peek().Type != TokenType.OpenParenthesis)
                {
                    Token op = opStack.Pop();
                    Node right = nodeStack.Pop();
                    Node left = nodeStack.Pop();
                    nodeStack.Push(new OperatorNode(left, right, op));
                }
                opStack.Pop(); // Remove the '(' from the stack
            }
        }

        while (opStack.Any())
        {
            Token op = opStack.Pop();
            Node right = nodeStack.Pop();
            Node left = nodeStack.Pop();
            nodeStack.Push(new OperatorNode(left, right, op));
        }

        return nodeStack.Pop(); // The root node of the expression tree
    }


    /// <summary>
    /// Defines a variable used by the <see cref="MathSolver"/> when calling <see cref="Solve(string)"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Define(string name, double value)
    {
        Variable var = new(name, "", AccessControl.Public);
        var.Value = value;

        userDefinedVariables.DeclareVariable(var);
    }
    /// <summary>
    /// Removes the definition of a variable used by the <see cref="MathSolver"/>
    /// </summary>
    /// <param name="name"></param>
    public static void Undefine(string name)
    {
        var toDelete = userDefinedVariables.Variables.FirstOrDefault(var => var.Name == name);
        if (toDelete != null)
            userDefinedVariables.Variables.Remove(toDelete);
    }

    /// <summary>
    /// A node in the math expression tree
    /// </summary>
    public record Node;
    /// <summary>
    /// A node that represents an operand (a number or variable)
    /// </summary>
    /// <param name="Value"></param>
    public record OperandNode(EquationResult Value) : Node;
    /// <summary>
    /// A node that represents an operator
    /// </summary>
    /// <param name="Left"></param>
    /// <param name="Right"></param>
    /// <param name="Operator"></param>
    public record OperatorNode(Node Left, Node Right, Token Operator) : Node;
}