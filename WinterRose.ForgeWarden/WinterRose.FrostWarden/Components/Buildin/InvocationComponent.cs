using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;

namespace WinterRose.ForgeWarden;
public class InvocationComponent : Component, IUpdatable, IRenderable
{
    public VoidInvocation<InvocationComponent> OnAwake { get; set; }
    public VoidInvocation<InvocationComponent> OnStart { get; set; }
    public VoidInvocation<InvocationComponent> OnDestroyEvent { get; set; }
    public VoidInvocation<InvocationComponent> OnVanishEvent { get; set; }
    public VoidInvocation<InvocationComponent> OnUpdate { get; set; }
    public VoidInvocation<InvocationComponent, Matrix4x4> OnDraw { get; set; }

    protected override void Awake()
    {
        base.Awake();
        OnAwake?.Invoke(this);
    }
    protected override void Start()
    {
        base.Start();
        OnStart?.Invoke(this);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnDestroyEvent?.Invoke(this);
    }
    protected override void OnVanish()
    {
        base.OnVanish();
        OnVanishEvent?.Invoke(this);
    }
    public void Update()
    {
        OnUpdate?.Invoke(this);
    }
    public void Draw(Matrix4x4 viewMatrix)
    {
        OnDraw?.Invoke(this, viewMatrix);
    }

}
