using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeCodex.Storage.Serialization;

public class CodexWFSerializer : DatabaseSerializer
{
    const bool DEFAULT_COMPRESS_STREAMS = false;
    const TargetFormat DEFAULT_FORMAT = TargetFormat.ReadableIntermediateRepresentation;
    public override void Serialize(Stream destination, CodexDatabase database)
    {
        bool compress = WinterForge.CompressedStreams;
        WinterForge.CompressedStreams = DEFAULT_COMPRESS_STREAMS;
        WinterForge.SerializeToStream(database, destination, DEFAULT_FORMAT);
        WinterForge.CompressedStreams = compress;
    }
    public override void Serialize(Stream destination, Table table)
    {
        bool compress = WinterForge.CompressedStreams;
        WinterForge.CompressedStreams = DEFAULT_COMPRESS_STREAMS;
        WinterForge.SerializeToStream(table, destination, DEFAULT_FORMAT);
        WinterForge.CompressedStreams = compress;
    }

    public override bool Deserialize(Stream source, [NotNullWhen(true)] out CodexDatabase? database)
    {
        bool compress = WinterForge.CompressedStreams;
        WinterForge.CompressedStreams = DEFAULT_COMPRESS_STREAMS;
        database = WinterForge.DeserializeFromHumanReadableStream<CodexDatabase>(source);
        WinterForge.CompressedStreams = compress;
        return database is not null;
    }
    public override bool Deserialize(Stream source, [NotNullWhen(true)] out Table? table)
    {
        bool compress = WinterForge.CompressedStreams;
        WinterForge.CompressedStreams = DEFAULT_COMPRESS_STREAMS;
        table = WinterForge.DeserializeFromHumanReadableStream<Table>(source);
        WinterForge.CompressedStreams = compress;
        return table is not null;
    }
}
