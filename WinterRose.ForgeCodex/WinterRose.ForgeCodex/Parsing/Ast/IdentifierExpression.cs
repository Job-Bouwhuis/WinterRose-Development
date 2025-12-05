using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class IdentifierExpression : Expression
    {
        public string Identifier { get; }
        public IdentifierExpression(string identifier) => Identifier = identifier;
        public override object? Evaluate(EvaluationContext context) => context.GetValue(Identifier);
    }
}
