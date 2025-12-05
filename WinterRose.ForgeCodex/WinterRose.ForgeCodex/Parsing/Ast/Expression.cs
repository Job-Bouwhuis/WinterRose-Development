using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public abstract class Expression : IEvaluable
    {
        public abstract object? Evaluate(EvaluationContext context);
    }
}
