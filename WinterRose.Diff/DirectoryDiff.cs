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
        WinterForge.SerializeToFile(this, path, TargetFormat.FormattedHumanReadable);
    }

    // TODO: swap implementation with a far more optimized format
    
    public static DirectoryDiff Load(string path)
    {
        return WinterForge.DeserializeFromHumanReadableFile<DirectoryDiff>(path);
    }

    public static DirectoryDiff Load(Stream stream)
    {
        return WinterForge.DeserializeFromHumanReadableStream<DirectoryDiff>(stream);
    }
}
