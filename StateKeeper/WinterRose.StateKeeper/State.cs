namespace WinterRose.StateKeeper;

public abstract class State(string name)
{
    public string Name => name;

    // Called once when entering the state
    public abstract void StateEnter(StateMachine stateMachine);

    // Called once when leaving the state
    public abstract void StateExit(StateMachine stateMachine);

    // Called every tick/update
    public abstract void StateUpdate(StateMachine stateMachine);
}
