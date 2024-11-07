using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Worlds
{
    /// <summary>
    /// Data and information about what chunk, or chunks, an object is in.
    /// <br></br>An object can be in multiple chunks at once if it is big enough, or parts of it are in different chunks.
    /// </summary>
    public class ObjectChunkData
    {
        /// <summary>
        /// The object that this data is about
        /// </summary>
        public WorldObject WorldObject { get; internal set; }

        /// <summary>
        /// The chunk in which the origin of the object is
        /// </summary>
        public WorldChunk ChunkContainingObjectOrigin { get; internal set; }

        /// <summary>
        /// A list of chunks that this object is in.
        /// </summary>
        public List<WorldChunk> Chunks { get; internal set; } = [];
    }
}
