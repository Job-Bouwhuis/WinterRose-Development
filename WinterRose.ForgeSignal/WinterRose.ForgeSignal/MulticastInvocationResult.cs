using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeSignal;
/// <summary>
/// The result of a raised MulticastInvocation
/// </summary>
/// <typeparam name="T"></typeparam>
public class MulticastInvocationResult<T> : IReadOnlyList<T>
{
    private readonly List<T> results = new();

    internal void AddResult(T result) => results.Add(result);

    public T this[int index] => results[index];
    public int Count => results.Count;
    public IEnumerator<T> GetEnumerator() => results.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class MulticastBooleanExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static MulticastBooleanVote Vote(this MulticastInvocationResult<bool> result)
    {
        int forCount = result.Count(r => r);
        int againstCount = result.Count - forCount;

        if (forCount > againstCount) return MulticastBooleanVote.For;
        if (againstCount > forCount) return MulticastBooleanVote.Against;
        return MulticastBooleanVote.Tie;
    }
}

public enum MulticastBooleanVote
{
    For,
    Against,
    Tie
}