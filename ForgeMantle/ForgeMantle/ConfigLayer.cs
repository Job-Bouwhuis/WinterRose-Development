using WinterRose.ForgeMantle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeMantle.Models;
using WinterRose.ForgeMantle.Values;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeMantle;
public class ConfigLayer
{
    public bool IsWritable { get; init; } = true;
    public IConfigStorage Storage { get; private set; }

    private ConfigSnapshot current = new();
    private ConfigSnapshot? staged = null;

    public void Stage(Action<ConfigSnapshot> updates)
    {
        staged ??= CloneSnapshot(current);
        updates(staged);
    }

    public ConfigLayer(IConfigStorage storage) => Storage = storage;

    private ConfigLayer() { } // for serialization

    public void Apply()
    {
        if (staged == null)
            return;

        current = staged;
        staged = null;
    }

    public (bool Found, object? Value) TryGet(string key)
    {
        if (current.State.TryGetValue(key, out var value))
            return (true, value);
        return (false, null);
    }

    public ConfigLayer Clone()
    {
        return new ConfigLayer
        {
            IsWritable = IsWritable,
            Storage = Storage,
            current = CloneSnapshot(current)
        };
    }

    private ConfigSnapshot CloneSnapshot(ConfigSnapshot input)
    {
        return new ConfigSnapshot
        {
            State = DeepCopy(input.State)
        };
    }

    private Dictionary<string, IConfigValue> DeepCopy(Dictionary<string, IConfigValue> source)
    {
        using MemoryStream mem = new MemoryStream();
        WinterForge.SerializeToStream(source, mem);
        mem.Position = 0;
        return WinterForge.DeserializeFromStream<Dictionary<string, IConfigValue>>(mem);
    }

    public void Save()
    {
        Storage.Save(current);
    }

    public void Restore()
    {
        Forge.Expect(staged).Null();
        staged = Storage.Load();
    }
}
