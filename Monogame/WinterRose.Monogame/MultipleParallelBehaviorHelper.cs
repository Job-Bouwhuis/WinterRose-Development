using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;
internal class MultipleParallelBehaviorHelper : IClearDisposable
{
    public List<ParallelBehaviorHelper> parallelBehaviorHelpers = [];

    public bool IsDisposed { get; private set; }

    public void Add(ParallelBehaviorHelper helper)
    {
        parallelBehaviorHelpers.Add(helper);
    }

    public bool Add(ObjectBehavior behavior)
    {
        Type type = behavior.GetType();

        foreach (var helper in parallelBehaviorHelpers)
            if(helper.Type == type)
                return helper.Add(behavior);

        ParallelBehaviorHelper h = new(type);
        h.Add(behavior);
        parallelBehaviorHelpers.Add(h);
        return true;
    }

    public void Dispose()
    {
        IsDisposed = true;
        foreach (var helper in parallelBehaviorHelpers)
            helper.Dispose();

        parallelBehaviorHelpers.Clear();
    }

    public void Execute()
    {
        foreach(var helper in parallelBehaviorHelpers)
            helper.Execute();
    }

    public static MultipleParallelBehaviorHelper operator +(MultipleParallelBehaviorHelper a, MultipleParallelBehaviorHelper b)
    { 
        foreach (ParallelBehaviorHelper helper in b.parallelBehaviorHelpers)
        {
            var existing = a.parallelBehaviorHelpers.Where(h => h.Type == helper.Type).FirstOrDefault();

            if (existing is null)
                a.parallelBehaviorHelpers.Add(helper);
            else
                foreach (var m in helper.behaviors)
                    existing.Add(m);
        }

        return a;
    }

}
