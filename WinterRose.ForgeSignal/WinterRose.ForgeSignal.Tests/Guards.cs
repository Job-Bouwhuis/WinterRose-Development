using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
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
        var e = Invocation.CreateMulticast();
        using var sub = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub1 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub2 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub3 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub4 = e.Subscribe(Invocation.Create(() => { got++; }));
        using var sub5 = e.Subscribe(Invocation.Create(() => { got++; }));
        ((Invocation)e).Invoke();
        Forge.Expect(got).EqualTo(6);
    }
}
