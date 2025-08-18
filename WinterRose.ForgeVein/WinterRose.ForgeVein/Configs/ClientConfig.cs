using WinterRose.ForgeMantle.Values;

namespace WinterRose.ForgeVein.Configs;

public class ClientConfig : IConfigValue
{
    [WFInclude] public string Username { get; set; } = "Player";
    [WFInclude] public string ServerAddress { get; set; } = "127.0.0.1";
    [WFInclude] public int ServerTcpPort { get; set; } = 53800;

    public object? Get() => this;
    public Type ValueType => typeof(ClientConfig);
}

