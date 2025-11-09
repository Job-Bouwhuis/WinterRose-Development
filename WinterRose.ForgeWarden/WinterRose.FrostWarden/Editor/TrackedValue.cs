using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Editor;
public class TrackedValue
{
    public object Owner => owner;

    public string MemberName { get; }
    private ReflectionHelper reflection;
    private MemberData memberData;

    private object? val;
    private object owner;

    public object? Value => val;

    /// <summary>
    /// Returns the value by ref, do not use for tracked values backed by properties!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ref T GetValueRef<T>()
    {
        return ref memberData.GetValueRef<object, T>(ref owner);
    }

    public TrackedValue(object owner, string memberName)
    {
        this.owner = owner;
        MemberName = memberName;
        reflection = new ReflectionHelper(owner);
        memberData = reflection.GetMember(memberName) 
            ?? throw new InvalidOperationException($"Member doesnt exist '{memberName}' on {owner.GetType().FullName}");
        val = memberData.GetValue(Owner);
    }

    internal bool HasValueChanged()
    {
        object? v = memberData.GetValue(Owner);
        if (!(v is null && val is null) && !Equals(v, val))
        {
            val = v;
            return true;
        }
        return false;
    }

    internal void Set(object? newVal)
    {
        memberData.SetValue(Owner, newVal);
        val = newVal;
    }
}
