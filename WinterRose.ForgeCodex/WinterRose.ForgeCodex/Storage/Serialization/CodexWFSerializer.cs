using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeCodex.Storage.Serialization;

public class CodexWFSerializer : DatabaseSerializer
{
    public override void Serialize(Stream destination, CodexDatabase database)
    {
        WinterForge.SerializeToStream(database, destination, TargetFormat.FormattedHumanReadable);
    }
    public override void Serialize(Stream destination, Table table)
    {
        WinterForge.SerializeToStream(table, destination, TargetFormat.FormattedHumanReadable);
    }

    public override bool Deserialize(Stream source, [NotNullWhen(true)] out CodexDatabase? database)
    {
        try
        {
            database = WinterForge.DeserializeFromHumanReadableStream<CodexDatabase>(source);
        }
        catch
        {
            database = null;
        }
        return database is not null;
    }
    public override bool Deserialize(Stream source, [NotNullWhen(true)] out Table? table)
    {
        //try
        //{
            table = WinterForge.DeserializeFromHumanReadableStream<Table>(source);
        //}
        //catch (Exception e)
        //{
        //    table = null;
        //}
        return table is not null;
    }
}
