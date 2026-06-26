using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Diff;

public enum FileState
{
    Unchanged,
    Modified,
    Added,
    Deleted
}

public class FileDiff
{
    [WFInclude]
    internal List<Op> ops;

    public FileState State { get; set; }
    public string NewFileHash { get; set; }

    public IReadOnlyList<Op> Operations => ops;

    public FileDiff()
    {
        ops = new List<Op>();
        State = FileState.Unchanged;
    }

    public FileDiff(FileState state, List<Op> operations)
    {
        State = state;
        ops = operations ?? new List<Op>();
    }

    public FileDiff(List<Op> operations)
    {
        ops = operations ?? new List<Op>();
        State = ops.Count is 0 ? FileState.Unchanged : FileState.Modified;
    }

    public void Save(string path)
    {
        WinterForge.SerializeToFile(this, path);
    }

    public static FileDiff Load(string path)
    {
        return WinterForge.DeserializeFromFile<FileDiff>(path);
    }
}
