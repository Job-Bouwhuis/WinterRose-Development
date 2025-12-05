using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace WinterRose.ForgeCodex.Storage.Serialization;

/// <summary>
/// Used to define serialziation behavior for the database
/// </summary>
public abstract class DatabaseSerializer
{
    public abstract void Serialize(Stream destination, CodexDatabase database);
    public abstract bool Deserialize(Stream source, [NotNullWhen(true)] out CodexDatabase? database);
    public abstract void Serialize(Stream destination, Table table);
    public abstract bool Deserialize(Stream source, [NotNullWhen(true)] out Table? table);
}
