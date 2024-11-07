using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Objects.Base;

namespace WinterRose.CrystalScripting.Legacy.Interpreting.TokenHandlers
{
    internal static class CrystalClassDefinitionHandler
    {
        public static List<Token> GetClassBodyHandler(List<Token> tokens, ref int startIndex, out CrystalError error)
        {
            error = CrystalError.NoError;
            int braceCount = 0;
            int currentIndex = startIndex;
            List<Token> bodyTokens = new List<Token>();

            // Find the opening brace
            while (currentIndex < tokens.Count)
            {
                Token currentToken = tokens[currentIndex];
                if (currentToken.Type == TokenType.LeftBrace)
                {
                    braceCount++;
                    break;
                }
                currentIndex++;
            }

            // Find the corresponding closing brace
            while (currentIndex < tokens.Count)
            {
                Token currentToken = tokens[currentIndex];
                if (currentToken.Type == TokenType.LeftBrace)
                {
                    braceCount++;
                }
                else if (currentToken.Type == TokenType.RightBrace)
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        break;
                    }
                }
                bodyTokens.Add(currentToken);
                currentIndex++;
            }

            return bodyTokens;
        }
    }
}
