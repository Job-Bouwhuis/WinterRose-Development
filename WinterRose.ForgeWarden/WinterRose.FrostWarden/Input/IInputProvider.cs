namespace WinterRose.ForgeWarden.Input;

/// <summary>
/// A common interface for different ways to provide input to a <see cref="InputContext"/>
/// </summary>
public interface IInputProvider
{
    /// <summary>
    /// Did this input start this frame
    /// </summary>
    /// <param name="binding"></param>
    /// <returns>True if yes, otherwise false</returns>
    bool IsPressed(InputBinding binding);
    /// <summary>
    /// Is this input currently held down
    /// </summary>
    /// <param name="binding"></param>
    /// <returns>True if yes, otherwise false</returns>
    bool IsDown(InputBinding binding);
    /// <summary>
    /// Was this input released this frame
    /// </summary>
    /// <param name="binding"></param>
    /// <returns>True if yes, otherwise false</returns>
    bool IsUp(InputBinding binding);

    /// <summary>
    /// Gets a numerical value for input. eg controller joysticks. Works with non analog inputs too giving a flat 0 or 1
    /// </summary>
    /// <param name="binding"></param>
    /// <returns></returns>
    float GetValue(InputBinding binding);

    void Update();

    /// <summary>
    /// The current position of the mouse. Will be -1 -1 when the mouse is not hovering the window
    /// </summary>
    Vector2 MousePosition { get; }
    /// <summary>
    /// The mouse move delta since the last frame
    /// </summary>
    Vector2 MouseDelta { get; }
    /// <summary>
    /// The delta of a scrollwheel this frame
    /// </summary>
    float ScrollDelta { get; }
}
