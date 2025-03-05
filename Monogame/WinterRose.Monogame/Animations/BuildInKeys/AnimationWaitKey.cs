namespace WinterRose.Monogame.Animations;

/// <summary>
/// Animation key for waiting for a certain amount of time before continuing to the next key
/// </summary>
public class AnimationWaitKey : AnimationKey
{
    /// <summary>
    /// The amount of time to wait in seconds
    /// </summary>
    public float waitTime;
    float currentTime;

    public AnimationWaitKey(float seconds) : base($"Wait for {seconds} seconds key")
    {
        waitTime = seconds;
    }
    private AnimationWaitKey() : base("") { } // For serialization

    /// <summary>
    /// Evaluates if the animation has waited for the desired amount of time
    /// </summary>
    /// <returns></returns>
    public override bool EvaluateEnd()
    {
        return currentTime >= waitTime;
    }

    /// <summary>
    /// Cancels the key by setting the current time to the wait time making the key end the next step
    /// </summary>
    public override void KeyCancel()
    {
        currentTime = waitTime;
    }

    /// <summary>
    /// Not used in this key
    /// </summary>
    public override void KeyEnd()
    {
    }

    /// <summary>
    /// Steps the key by adding the time since the last frame to the current time
    /// </summary>
    public override void KeyStep()
    {
        currentTime += Time.deltaTime;
    }

    /// <summary>
    /// Not used in this key
    /// </summary>
    /// <param name="owner"></param>
    public override void Setup(WorldObject owner)
    {
    }

    /// <summary>
    /// Not used in this key
    /// </summary>
    public override void StartKey()
    {
    }

    /// <summary>
    /// Always returns true because this key does not use a target.
    /// </summary>
    /// <returns></returns>
    public override bool ValidateTarget()
    {
        return true;
    }
}
