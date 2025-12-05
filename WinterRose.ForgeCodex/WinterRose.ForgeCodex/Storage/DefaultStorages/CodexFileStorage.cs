using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WinterRose.ForgeCodex.Storage.Serialization;

namespace WinterRose.ForgeCodex.Storage.DefaultStorages;

/// <summary>
/// Stores the codex in a file
/// </summary>

public class CodexFileStorage : StorageProvider
{
    public override string Name => "File storage";

    public DatabaseSerializer Serializer { get; }

    DirectoryInfo databaseRoot;

    /// <summary>
    /// Creates a new codex file storage
    /// </summary>
    /// <param name="path">The path where the codex directory will be created. in wich each table will get its own file</param>
    /// <param name="serializer">The serializer used to save and load tables</param>
    public CodexFileStorage(string path, DatabaseSerializer serializer)
    {
        databaseRoot = new DirectoryInfo(path);
        Serializer = serializer;
    }

    public override void WriteTable(Table snapshot)
    {
        if (!databaseRoot.Exists)
            databaseRoot.Create();
        FileInfo? file = databaseRoot.GetFiles(snapshot.Name + ".table").FirstOrDefault();
        file ??= new FileInfo(databaseRoot.FullName + "\\" + snapshot.Name + ".table");
        using FileStream stream = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        Serializer.Serialize(stream, snapshot);
    }

    public override List<Table> ReadDatabase()
    {
        if (!databaseRoot.Exists)
            return [];
        FileInfo[] files = databaseRoot.GetFiles("*.table");
        List<Table> tables = new List<Table>();
        foreach(FileInfo file in files)
            tables.Add(ReadTable(file.FullName));
        return tables;
    }
    public override Table ReadTable(string table)
    {
        using FileStream file = File.OpenRead(table);
        if(Serializer.Deserialize(file, out Table? t))
            return t;
        throw new InvalidDataException("Table was not stored correctly");
    }
    public override void RemoveTable(string table)
    {
        string path = Path.Combine(databaseRoot.FullName, table + ".table");
        if (File.Exists(path))
            File.Delete(path);
    }
}
