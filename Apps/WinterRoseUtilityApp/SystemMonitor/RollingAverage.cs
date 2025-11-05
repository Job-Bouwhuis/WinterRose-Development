namespace WinterRoseUtilityApp.SystemMonitor;

internal class RollingAverage
{
    readonly Queue<float> samples = new();
    readonly int maxSamples;
    float sum;

    public RollingAverage(int maxSamples)
    {
        this.maxSamples = Math.Max(1, maxSamples);
    }

    public void AddSample(float value)
    {
        sum += value;
        samples.Enqueue(value);
        if (samples.Count > maxSamples)
            sum -= samples.Dequeue();
    }

    public float GetAverage()
    {
        return samples.Count > 0 ? sum / samples.Count : 0f;
    }
}
