using WinterRose.ForgeCodex.Evaluation;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public class QueryFrom : IEvaluable
    {
        public string SourceTypeName { get; }
        public Expression? Where { get; }

        protected IReadOnlyList<int> SelectedRows { get; private set; }

        public QueryFrom(string sourceTypeName, Expression? where = null)
        {
            SourceTypeName = sourceTypeName;
            Where = where;
        }

        /// <summary>
        /// Do not forget to call the base before doing anything else, itll 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual object? Evaluate(EvaluationContext context)
        {
            var db = context.Database;
            Table table = db.GetTable(SourceTypeName);
            var surviving = new List<int>();

            for (int i = 0; i < table.RowCount; i++)
            {
                var rowCtx = new EvaluationContext(db, table, i);

                if (Where == null)
                {
                    surviving.Add(i);
                    continue;
                }

                var result = Where.Evaluate(rowCtx);

                if (result is bool b && b)
                    surviving.Add(i);
            }

            SelectedRows = surviving;
            return surviving;
        }
    }
}
