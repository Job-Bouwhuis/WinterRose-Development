using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Evaluation;

public interface IEvaluable
{
    object? Evaluate(EvaluationContext context);
}
