using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.Exceptions;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{
    public sealed class CrystalConditionExpression
    {
        List<Token> tokens;
        CrystalScope scope;

        public CrystalConditionExpression(List<Token> tokens, CrystalScope scope)
        {
            this.tokens = tokens;
            this.scope = scope;
        }
        private CrystalConditionExpression() { }

        public bool Evaluate()
        {
            if (scope is null)
                throw new NotInitializedException(nameof(scope));

            CrystalTokenInterpreter interpreter = new(ref scope);
            var v = interpreter.ExecuteTokens(tokens);
            if (v is not null)
            {
                if (v.Type is CrystalBoolean b)
                {
                    return b;
                }
                else
                {
                    throw new CrystalInterpretingException(00005, $"Expected a boolean value, got {v.GetType().Name}");
                }
            }
            else
            {
                throw new CrystalInterpretingException(00006, "The condition expression returned null.");
            }
        }
    }
}
