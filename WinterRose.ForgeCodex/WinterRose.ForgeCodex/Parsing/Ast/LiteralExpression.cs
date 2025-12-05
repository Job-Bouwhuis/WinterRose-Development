using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class LiteralExpression : Expression
    {
        public object? Value { get; }
        public LiteralExpression(object? value) => Value = value;

        public override object? Evaluate(EvaluationContext context) => Value;
    }
}
