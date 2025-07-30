namespace ForgeMantle.Values;

public interface IConfigValue
{
    object? Get();
    Type ValueType { get; }
}


