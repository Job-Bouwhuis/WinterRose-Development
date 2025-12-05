using System.Collections;
using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }

        public BinaryExpression(Expression left, string op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override object? Evaluate(EvaluationContext ctx)
        {
            var leftVal = Left.Evaluate(ctx);
            var rightVal = Right.Evaluate(ctx);

            return Operator switch
            {
                "==" => Equals(leftVal, rightVal),
                "!=" => !Equals(leftVal, rightVal),
                ">" => Comparer.Default.Compare(leftVal, rightVal) > 0,
                "<" => Comparer.Default.Compare(leftVal, rightVal) < 0,
                ">=" => Comparer.Default.Compare(leftVal, rightVal) >= 0,
                "<=" => Comparer.Default.Compare(leftVal, rightVal) <= 0,
                "&&" => (bool)leftVal! && (bool)rightVal!,
                "||" => (bool)leftVal! || (bool)rightVal!,
                _ => throw new NotSupportedException($"Unsupported operator {Operator}")
            };
        }
    }
}
