using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

namespace WinterRose.StateKeeper;
/// <summary>
/// Provides <see cref="VoidInvocation"/> hooks for the different state methods
/// </summary>
public class InvocationState : State
{
    public VoidInvocation<StateMachine> OnStateEnter { get; }
    public VoidInvocation<StateMachine> OnStateExit { get; }
    public VoidInvocation<StateMachine> OnStateUpdate { get; }

    public InvocationState(string name, VoidInvocation<StateMachine> stateEnter, VoidInvocation<StateMachine> stateExit, VoidInvocation<StateMachine> stateUpdate)
        : base(name)
    {
        OnStateEnter = stateEnter;
        OnStateExit = stateExit;
        OnStateUpdate = stateUpdate;
    }

    public override void StateEnter(StateMachine stateMachine) => OnStateEnter.Invoke(stateMachine);
    public override void StateExit(StateMachine stateMachine) => OnStateExit.Invoke(stateMachine);
    public override void StateUpdate(StateMachine stateMachine) => OnStateUpdate.Invoke(stateMachine);
}
