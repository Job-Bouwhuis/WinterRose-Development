using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.CrystalScripting.Legacy.Objects.Types;

namespace WinterRose.CrystalScripting.Legacy.Interpreting.TokenHandlers
{
    public static class FunctionCallTokenHandler
    {
        public static void HandleFunctionCall(CrystalTokenizer tokenizer, int currentTokenIndex, Token token, CrystalClass currentClass)
        {
            // Get the function name from the identifier token
            string functionName = token.Lexeme;

            // Find the function in the current class;
            if (currentClass.Body.PublicIdeintifiers.TryGetFunction(functionName, out CrystalFunction function))
            {
                // Handle the case where the function is not found
                Console.WriteLine($"Error: Function '{functionName}' not found");
                return;
            }

            // Get the argument tokens between parentheses
            List<Token> argumentTokens = GetArgumentTokens(tokenizer, token);

            // Evaluate the arguments
            List<CrystalVariable> arguments = EvaluateArguments(argumentTokens, currentClass);

            // Call the function with the evaluated arguments
            CrystalType result = CrystalType.FromObject(function.Invoke(arguments.ToArray()));

            // Do something with the result if needed
            Console.WriteLine($"Function '{functionName}' called with arguments: {string.Join(", ", arguments.Select(a => a.Type))}");
            Console.WriteLine($"Result: {result}");
        }

        private static List<Token> GetArgumentTokens(CrystalTokenizer tokenizer, Token token)
        {
            // Get the index of the current token
            int currentIndex = tokenizer.Tokens.IndexOf(token);

            // Find the opening parenthesis
            int openParenthesisIndex = FindNextTokenOfType(tokenizer, TokenType.LeftParenthesis, currentIndex);

            // Find the closing parenthesis
            int closeParenthesisIndex = FindMatchingClosingParenthesis(tokenizer, openParenthesisIndex);

            // Get the tokens between parentheses
            List<Token> argumentTokens = tokenizer.Tokens.GetRange(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);

            return argumentTokens;
        }

        private static int FindNextTokenOfType(CrystalTokenizer tokenizer, TokenType type, int startIndex)
        {
            for (int i = startIndex + 1; i < tokenizer.Tokens.Count; i++)
            {
                if (tokenizer.Tokens[i].Type == type)
                {
                    return i;
                }
            }

            return -1; // Return -1 if token type is not found
        }

        private static int FindMatchingClosingParenthesis(CrystalTokenizer tokenizer, int openParenthesisIndex)
        {
            int nestedParenthesisCount = 0;

            for (int i = openParenthesisIndex + 1; i < tokenizer.Tokens.Count; i++)
            {
                if (tokenizer.Tokens[i].Type == TokenType.LeftParenthesis)
                {
                    nestedParenthesisCount++;
                }
                else if (tokenizer.Tokens[i].Type == TokenType.RightParenthesis)
                {
                    if (nestedParenthesisCount == 0)
                    {
                        return i;
                    }

                    nestedParenthesisCount--;
                }
            }

            return -1; // Return -1 if matching closing parenthesis is not found
        }

        private static List<CrystalVariable> EvaluateArguments(List<Token> argumentTokens, CrystalClass currentClass)
        {
            List<CrystalVariable> arguments = new List<CrystalVariable>();

            foreach (Token argumentToken in argumentTokens)
            {
                // Evaluate each argument token and add the result to the arguments list
                CrystalVariable argumentValue = EvaluateExpression(argumentToken, currentClass);
                arguments.Add(argumentValue);
            }

            return arguments;
        }

        private static CrystalVariable EvaluateExpression(Token expressionToken, CrystalClass currentClass)
        {
            // Evaluate the expression and return the result
            // Implementation depends on your expression evaluation logic
            CrystalType evaluatedValue = CrystalType.FromObject(null);
            CrystalVariable variable = new CrystalVariable(expressionToken.Lexeme, evaluatedValue);
            return variable;
        }
    }
}
