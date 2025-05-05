using WinterRose;
using WinterRose.NetworkServer.Packets;

internal class SetUsernamePacket : Packet
{
    private SetUsernamePacket() { } // for serialization
    public SetUsernamePacket(string name)
    {
        Header = new BasicHeader("SETUSERNAME");
        Content = new StringContent(name);
    }
}