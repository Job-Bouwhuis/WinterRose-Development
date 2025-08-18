using System;

namespace WinterRose.NetworkServer.Packets;

public class ClusterHelloPacket : Packet
{
    public class ClusterHelloContent : PacketContent
    {
        public Guid NodeId { get; set; }
        public string ClusterId { get; set; }
        public string Version { get; set; }

        public ClusterHelloContent(Guid nodeId, string clusterId, string version)
        {
            NodeId = nodeId;
            ClusterId = clusterId;
            Version = version;
        }

        private ClusterHelloContent() { } // for serialization
    }

    public ClusterHelloPacket(Guid nodeId, string clusterId, string version)
    {
        Header = new BasicHeader("ClusterHello");
        Content = new ClusterHelloContent(nodeId, clusterId, version);
    }

    private ClusterHelloPacket() { } // for serialization
}