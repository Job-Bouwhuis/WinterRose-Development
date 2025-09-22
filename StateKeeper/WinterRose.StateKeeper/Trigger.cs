namespace WinterRose.StateKeeper;

public abstract class Trigger
{
    // Returns true if this trigger is "active" / should cause a state change
    public abstract bool IsTriggered();

    // The state to transition to if triggered
    public State TargetState { get; }

    protected Trigger(State targetState)
    {
        TargetState = targetState;
    }
}
