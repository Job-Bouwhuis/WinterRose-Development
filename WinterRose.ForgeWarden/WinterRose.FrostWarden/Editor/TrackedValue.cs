using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Editor;
public class TrackedValue
{
    public object Owner { get; }
    public string MemberName { get; }
    private ReflectionHelper reflection;
    private MemberData memberData;

    private object? val;

    public object? Value => val;

    public TrackedValue(object owner, string memberName)
    {
        Owner = owner;
        MemberName = memberName;
        reflection = new ReflectionHelper(owner);
        memberData = reflection.GetMember(memberName) 
            ?? throw new InvalidOperationException($"Member doesnt exist '{memberName}' on {owner.GetType().FullName}");
        val = memberData.GetValue(Owner);
    }

    public bool HasValueChanged()
    {
        object? v = memberData.GetValue(Owner);
        if (!(v is null && val is null) && !Equals(v, val))
        {
            val = v;
            return true;
        }
        return false;
    }
}
