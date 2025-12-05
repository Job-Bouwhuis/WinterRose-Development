namespace WinterRose.ForgeCodex.Storage;

public abstract class StorageProvider
{
    public abstract string Name { get; }

    /// <summary>
    /// Called when the codex assumes this provider as active
    /// </summary>
    public virtual void OnAttach() { }

    /// <summary>
    /// Called when shutting down or switching providers
    /// </summary>
    public virtual void OnDetach() { }

    /// <summary>
    /// Write the entire DB to storage
    /// </summary>
    /// <param name="snapshot"></param>
    public virtual void WriteDatabase(CodexDatabase snapshot)
    {
        foreach(var table in snapshot.GetTables())
            WriteTable(table);
    }

    /// <summary>
    /// Write this specific 
    /// </summary>
    /// <param name="snapshot"></param>
    public abstract void WriteTable(Table snapshot);

    public virtual void Flush() { }

    public virtual void Validate(CodexDatabase snapshot) { }

    public abstract void RemoveTable(string table);

    public abstract List<Table> ReadDatabase();
    public abstract Table ReadTable(string table);
}
