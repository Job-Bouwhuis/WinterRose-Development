using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeGuardChecks.Exceptions;
using WinterRose.ForgeThread;

namespace WinterRose.ForgeSignal.Tests;
[GuardClass("ForgeSignal.Basic")]
public class ForgeSignalBasicGuards
{
    [Guard]
    public void FiresAndReceives()
    {
        int got = 0;
        var _ = Invocation.Create<int>(i => got = i);
        _.Invoke(123);
        Forge.Expect(got).EqualTo(123);
    }

    [Guard]
    public void FiresAndReturns()
    {
        int got = 0;
        var e = Invocation.Create<int, int>(i =>
        {
            got = i;
            return i;
        });
        int returned = e.Invoke(123);
        Forge.Expect(got).EqualTo(123);
    }

    [Guard]
    public void MultiCastFiresAndReturns()
    {
        var e = Invocation.CreateMulticast<int, int>();
        using var sub = e.Subscribe(Invocation.Create((int i) =>
        {
            return i + 1;
        }));
        using var sub2 = e.Subscribe(Invocation.Create((int i) =>
        {
            return i;
        }));
        using var sub3 = e.Subscribe(Invocation.Create((int i) =>
        {
            return i - 1;
        }));
        var returned = e.Invoke(0);
    }

    [Guard]
    public void MultiCastFiresAndReturnsBool()
    {
        var e = Invocation.CreateMulticast<bool>();
        using var sub = e.Subscribe(Invocation.Create(() =>
        {
            return true;
        }));
        using var sub2 = e.Subscribe(Invocation.Create(() =>
        {
            return true;
        }));
        using var sub3 = e.Subscribe(Invocation.Create(() =>
        {
            return false;
        }));
        var returned = e.Invoke();
        Forge.Expect(returned.Vote() == MulticastBooleanVote.For).True();
    }

    [Guard]
    public void FiresAndThrowsWrongArgCount()
    {
        void test()
        {
            int got = 0;
            var _ = Invocation.Create<int>(i => got = i);
            _.Invoke(123, 1);
        }

        Forge.Expect(test).WhenCalled().ToThrow<InvalidOperationException>();
    }

    [Guard]
    public void FiresAndThrowsWrongArgType()
    {
        void test()
        {
            int got = 0;
            var _ = Invocation.Create<int>(i => got = i);
            _.Invoke(123f);
        }

        Forge.Expect(test).WhenCalled().ToThrow<InvalidOperationException>();
    }

    [Guard]
    public void MulticastFiresAndReceives()
    {
        int got = 0;
        var e = Invocation.CreateVoidMulticast();
        using var sub = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub1 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub2 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub3 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub4 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub5 = e.Subscribe(Invocation.Create(() => { got++; }));
        ((Invocation)e).Invoke();
        Forge.Expect(got).EqualTo(6);
    }

    [Guard]
    public void InvocationHooksAreCalled()
    {
        bool main = false;
        bool before = false;
        bool after = false;
        bool Finally = false;

        Invocation.Create(() => main = true)
            .Before(Invocation.Create(() => before = true))
            .After(Invocation.Create(() => after = true))
            .Finally(Invocation.Create(() => Finally = true))
            .Invoke();

        Forge.Expect(main).True();
        Forge.Expect(before).True();
        Forge.Expect(after).True();
        Forge.Expect(Finally).True();
    }
}
