using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Storage;

public sealed class StorageCoordinator
{
    private StorageProvider provider;

    public void setProvider(StorageProvider newProvider)
    {
        provider?.OnDetach();
        provider = newProvider;
        provider?.OnAttach();
    }

    public void writeFull(CodexDatabase snapshot)
    {
        provider?.Validate(snapshot);
        provider?.WriteDatabase(snapshot);
    }

    public void writeTable(Table snapshot)
    {
        provider?.WriteTable(snapshot);
    }

    public void flush()
    {
        provider?.Flush();
    }
}
