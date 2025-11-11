namespace WinterRose.ForgeWarden.TileMaps;

public enum LoadedState
{
    Unloaded,   // not present in memory
    Loading,    // in the process of loading (async or sync)
    Loaded,     // loaded into memory but not actively updated/drawn
    Active,     // within camera active radius: updated & drawn
    Persisted   // explicitly requested to remain active regardless of camera
}
