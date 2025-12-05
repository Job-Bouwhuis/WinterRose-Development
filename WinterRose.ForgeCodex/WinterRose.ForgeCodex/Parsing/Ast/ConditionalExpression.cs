using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast;

internal class ConditionalExpression : Expression
{
    public Expression Condition { get; set; }
    public Expression TrueExpr { get; set; }
    public Expression FalseExpr { get; set; }

    public ConditionalExpression(Expression condition, Expression trueExpr, Expression falseExpr)
    {
        Condition = condition;
        TrueExpr = trueExpr;
        FalseExpr = falseExpr;
    }

    public override object? Evaluate(EvaluationContext ctx)
    {
        object condRes = Condition.Evaluate(ctx);
        if (condRes is not bool b)
            throw new InvalidOperationException("Condition resulted in a non boolean value");

        return b ? TrueExpr.Evaluate(ctx) : FalseExpr.Evaluate(ctx);
    }
}