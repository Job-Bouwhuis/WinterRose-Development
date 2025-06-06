using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

internal class ParallelBehaviorHelper(Type type) : IClearDisposable
{
    public Type Type { get; } = type;

    public bool IsDisposed { get; private set; }

    public List<ObjectBehavior> behaviors = new List<ObjectBehavior>();

    public bool Add(ObjectBehavior behavior)
    {
        if (behavior.GetType() != type) return false;
        if(behaviors.Contains(behavior)) return false;
        behaviors.Add(behavior);
        return true;
    }

    public void Execute()
    {
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.ForEach(behaviors,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            behavior =>
            {
                try
                {
                    behavior.CallUpdate();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

        if (!exceptions.IsEmpty)
        {
            // You could throw an AggregateException or just log them
            throw new AggregateException(exceptions);
        }
    }

    public void Dispose()
    {
        IsDisposed = true;
        behaviors.Clear();
    }

    public static ParallelBehaviorHelper operator +(ParallelBehaviorHelper a, ParallelBehaviorHelper b)
    {
        if (a.Type != b.Type)
            throw new InvalidOperationException("The two parallel helpers have different component types. cant merge them");

        foreach(var bb in b.behaviors)
            a.Add(bb);

        return a;
    }
}
