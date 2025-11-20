using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Diff;

public class FileDiff
{
    [WFInclude]
    internal List<DiffEngine.Op> ops;
    
    public IReadOnlyList<DiffEngine.Op> Operations => ops;

    public static implicit operator FileDiff(List<DiffEngine.Op> ops) => new FileDiff { ops = ops };

    public void Save(string path)
    {
        WinterForge.SerializeToFile(this, path);
    }

    public static FileDiff Load(string path)
    {
        return WinterForge.DeserializeFromFile<FileDiff>(path);
    }
}
