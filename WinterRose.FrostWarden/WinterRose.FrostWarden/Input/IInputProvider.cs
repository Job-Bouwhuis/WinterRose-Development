namespace WinterRose.ForgeWarden.Input;

public interface IInputProvider
{
    bool IsPressed(InputBinding binding);
    bool IsDown(InputBinding binding);
    bool IsUp(InputBinding binding);

    bool WasRepeated(InputBinding binding);
    bool WasRepeated(InputBinding binding, TimeSpan within);
    bool WasRepeated(InputBinding binding, int times);
    bool WasRepeated(InputBinding binding, int times, TimeSpan within);

    bool HeldFor(InputBinding binding, TimeSpan duration);

    float GetValue(InputBinding binding);

    void Update();

    Vector2 MousePosition { get; }
    Vector2 MouseDelta { get; }
}
