using System;
using System.Collections.Generic;

namespace WinterRose.Reflection;

public class MemberDataCollection<T>
{
    private readonly List<MemberData> members;

    public MemberDataCollection(ReflectionHelper<T> rh)
    {
        members = rh.GetMembers().members;
    }

    public MemberDataCollection(List<MemberData> members)
    {
        this.members = members;
    }

    /// <summary>
    /// Gets the member with the provided name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The found member</returns>
    /// <exception cref="FieldNotFoundException"></exception>
    public MemberData this[string name] => members.Find(x => x.Name == name) 
        ?? throw new FieldNotFoundException($"Field or property with name {name} not found.");

    /// <summary>
    /// Gets the <see cref="IEnumerator{T}"/> for this collection
    /// </summary>
    /// <returns></returns>
    public IEnumerator<MemberData> GetEnumerator() => members.GetEnumerator();

    internal bool TryGet(string name, out MemberData? memer)
    {
        try
        {
            return (memer = this[name]) != null;
        }
        catch
        {
            memer = null;
            return false;
        }
    }
}