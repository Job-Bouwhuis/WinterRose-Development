using WinterRose.StateKeeper;

public class StateMachine
{
    public State CurrentState { get; private set; }
    private readonly List<Trigger> globalTriggers = new();
    private readonly Dictionary<State, List<Trigger>> stateTriggers = new();

    public void AddTrigger(State fromState, Trigger trigger)
    {
        if (!stateTriggers.TryGetValue(fromState, out var list))
        {
            list = new List<Trigger>();
            stateTriggers[fromState] = list;
        }
        list.Add(trigger);
    }

    public void AddTrigger(Trigger trigger)
    {
        globalTriggers.Add(trigger);
    }

    public void SetInitialState(State state)
    {
        CurrentState = state;
        CurrentState.StateEnter(this);
    }

    public void Update()
    {
        if (CurrentState == null) return;

        foreach (var trigger in globalTriggers)
        {
            if (trigger.IsTriggered())
            {
                TransitionTo(trigger.TargetState);
                return;
            }
        }

        if (stateTriggers.TryGetValue(CurrentState, out var allowedTriggers))
        {
            foreach (var trigger in allowedTriggers)
            {
                if (trigger.IsTriggered())
                {
                    TransitionTo(trigger.TargetState);
                    return;
                }
            }
        }

        CurrentState.StateUpdate(this);
    }

    public void TransitionTo(State newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.StateExit(this);
        CurrentState = newState;
        CurrentState?.StateEnter(this);
    }
}
