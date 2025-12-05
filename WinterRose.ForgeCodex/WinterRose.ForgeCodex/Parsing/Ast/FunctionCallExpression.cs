using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast;

internal class FunctionCallExpression : Expression
{
    private string func;
    private List<Expression> expressions;

    public FunctionCallExpression(string func, List<Expression> expressions)
    {
        this.func = func;
        this.expressions = expressions;
    }

    public override object? Evaluate(EvaluationContext context) => throw new NotImplementedException();
}