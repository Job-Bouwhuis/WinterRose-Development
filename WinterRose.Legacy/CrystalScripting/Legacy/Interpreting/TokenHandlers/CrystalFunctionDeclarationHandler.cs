using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Objects.Base;

namespace WinterRose.CrystalScripting.Legacy.Interpreting.TokenHandlers
{
    public static class CrystalFunctionDeclarationHandler
    {
        public static List<Token> GetFunctionBodyTokens(CrystalTokenizer tokenizer, ref int index, out CrystalError? error)
        {
            error = null;
            List<Token> tokens = new();

            int depth = 0;
            while (index < tokenizer.Tokens.Count)
            {
                if (tokenizer.Tokens[index].Lexeme == "{")
                    depth++;
                else if (tokenizer.Tokens[index].Lexeme == "}")
                {
                    depth--;
                    if (depth < 0)
                        error = new CrystalError("Invalid Syntax.", "Found unexpected '}' before '{' in function decleration");
                    if (depth == 0)
                    {
                        tokens.Add(tokenizer.Tokens[index]);
                        break;
                    }
                }
                if (depth > 0)
                    tokens.Add(tokenizer.Tokens[index]);
                index++;
            }
            if (depth != 0)
                error = new CrystalError("Invalid Syntax.", "Expected '}' to close function decleration");

            return tokens;
        }
    }
}
