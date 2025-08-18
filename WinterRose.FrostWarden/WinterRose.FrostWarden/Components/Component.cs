using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Entities;

namespace WinterRose.ForgeWarden;

public abstract class Component : IComponent
{
    [WFInclude(Priority = int.MaxValue)]
    public Entity owner { get; internal set; }
    public Transform transform => owner.transform;

    public float awakeTime { get; private set; }
    public float startTime { get; private set; }
    public float vanishTime { get; private set; }
    public float destroyTime { get; private set; }

    protected virtual void Awake() { }
    protected virtual void Start() { }
    protected virtual void OnVanish() { }
    protected virtual void OnDestroy() { }

    internal void CallAwake()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Awake();
        stopwatch.Stop();
        awakeTime = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
    }

    internal void CallStart()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Start();
        stopwatch.Stop();
        startTime = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
    }

    internal void CallOnVanish()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        OnVanish();
        stopwatch.Stop();
        vanishTime = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
    }

    internal void CallOnDestroy()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        OnDestroy();
        stopwatch.Stop();
        destroyTime = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000f;
    }

    public T AddComponent<T>(params object[] args) where T : Component
    {
        return owner.AddComponent<T>(args);
    }

    public T AddComponent<T>(T component) where T : Component
       => owner.AddComponent(component);

    public void RemoveComponent<T>() where T : Component
        => owner.RemoveComponent<T>();

    public T? GetComponent<T>() where T : Component
        => owner.GetComponent<T>();

    public Component GetComponent(Type t)
        => owner.GetComponent(t);

    public bool HasComponent<T>() where T : Component
        => owner.HasComponent<T>();

    public IEnumerable<T> GetAllComponents<T>() where T : Component
        => owner.GetAllComponents<T>();
}
