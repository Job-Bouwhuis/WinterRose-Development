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
    public static class VariableTokenHandler
    {
        public static void HandleVariableDeclaration(List<Token> tokens, Token token, CrystalCodeBody variableBlock, CrystalCodeBody classBody)
        {
            // Extract the variable section tokens from the code block
            List<Token> variableSectionTokens = variableBlock.BodyTokens;

            // Remove leading and trailing whitespace tokens
            //variableSectionTokens = RemoveWhitespaceTokens(variableSectionTokens);

            //// Remove the "variables" keyword token
            //variableSectionTokens.RemoveAt(0);

            // Split the variable declarations using semicolon as the separator
            List<List<Token>> variableDeclarations = SplitVariableDeclarations(variableSectionTokens);

            foreach (List<Token> variableDeclaration in variableDeclarations)
            {
                // Process the individual variable declaration
                ProcessVariableDeclaration(token, variableDeclaration, classBody);
            }
        }

        //private static List<Token> RemoveWhitespaceTokens(List<Token> tokens)
        //{
        //    return tokens.Where(token => token.Type != TokenType.Whitespace).ToList();
        //}

        private static List<List<Token>> SplitVariableDeclarations(List<Token> tokens)
        {
            List<List<Token>> declarations = new List<List<Token>>();
            List<Token> currentDeclaration = new List<Token>();

            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Semicolon)
                {
                    if (currentDeclaration.Count > 0)
                    {
                        declarations.Add(currentDeclaration);
                        currentDeclaration = new List<Token>();
                    }
                }
                else if (token.Type == TokenType.RightBrace)
                    break;
                else
                {
                    currentDeclaration.Add(token);
                }
            }

            if (currentDeclaration.Count > 0)
            {
                declarations.Add(currentDeclaration);
            }

            return declarations;
        }

        private static void ProcessVariableDeclaration(Token token, List<Token> declarationTokens, CrystalCodeBody body)
        {
            // Extract the variable name
            string variableName = declarationTokens[0].Lexeme.Trim();

            // Check if a default value is provided
            string defaultValue = null;
            if (declarationTokens.Count > 2 && declarationTokens[1].Type == TokenType.AssignVariable)
            {
                defaultValue = GetDefaultValue(declarationTokens.GetRange(2, declarationTokens.Count - 2));
            }

            // Create the variable object or update its default value
            SetVariable(variableName, defaultValue, body);
        }

        private static string GetDefaultValue(List<Token> defaultValueTokens)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Token token in defaultValueTokens)
            {
                builder.Append(token.Lexeme);
            }
            return builder.ToString().Trim();
        }

        private static bool VariableExists(string variableName, CrystalCodeBody body)
        {
            return body.PublicIdeintifiers.IdentifyerExistsWithinScope(variableName);
        }

        private static void SetVariable(string variableName, string defaultValue, CrystalCodeBody body)
        {
            if (TypeWorker.TryCastPrimitive(defaultValue, out double doubleValue))
            {
                body.PublicIdeintifiers.DeclareVariable(new CrystalVariable(variableName, CrystalType.FromObject(doubleValue)));
                return;
            }
            if (TypeWorker.TryCastPrimitive(defaultValue, out bool boolValue))
            {
                // TODO: implement bool variable type
            }
            if (TypeWorker.TryCastPrimitive(defaultValue, out string stringValue))
            {
                body.PublicIdeintifiers.DeclareVariable(new CrystalVariable(variableName, CrystalType.FromObject(stringValue)));
                return;
            }
            if (defaultValue is null)
            {
                body.PublicIdeintifiers.DeclareVariable(new CrystalVariable(variableName, CrystalType.FromObject(null)));
                return;
            }
        }
    }
}


