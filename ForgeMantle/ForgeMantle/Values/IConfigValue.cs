namespace WinterRose.ForgeMantle.Values;

public interface IConfigValue
{
    object? Get();
    Type ValueType { get; }
}


