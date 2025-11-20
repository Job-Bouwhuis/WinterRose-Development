using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Diff;

public class DirectoryDiff
{
    public Dictionary<string, FileDiff> FileDiffs { get; set; } = [];

    public void Save(string path)
    {
        WinterForge.SerializeToFile(this, path);
    }

    public static FileDiff Load(string path)
    {
        return WinterForge.DeserializeFromFile<FileDiff>(path);
    }
}
