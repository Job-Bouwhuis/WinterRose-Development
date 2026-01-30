using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

namespace WinterRose.ForgeWarden;
public class InvocationComponent : Component, IUpdatable, IRenderable
{
    [Hide]
    public VoidInvocation<InvocationComponent> OnAwake { get; set; }
    [Hide]
    public VoidInvocation<InvocationComponent> OnStart { get; set; }
    [Hide]
    public VoidInvocation<InvocationComponent> OnDestroyEvent { get; set; }
    [Hide]
    public VoidInvocation<InvocationComponent> OnVanishEvent { get; set; }
    [Hide]
    public VoidInvocation<InvocationComponent> OnUpdate { get; set; }
    [Hide]
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
