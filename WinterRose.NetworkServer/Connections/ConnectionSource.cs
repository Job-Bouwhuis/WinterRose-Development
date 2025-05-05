using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Connections
{
    /// <summary>
    /// Defines what source does the packet have
    /// </summary>
    public enum ConnectionSource
    {
        /// <summary>
        /// The source is unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// The packet was initiated by the server to a client
        /// </summary>
        Server,
        /// <summary>
        /// The packet was initiated by a client to the server
        /// </summary>
        Client,
        /// <summary>
        /// The packet was initiated by a client, meant for another client
        /// </summary>
        ClientRelay
    }
}
