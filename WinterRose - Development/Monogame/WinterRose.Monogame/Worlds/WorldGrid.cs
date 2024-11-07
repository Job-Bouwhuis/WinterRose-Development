using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Worlds
{
    /// <summary>
    /// A grid of world chunks containing data where what objects are in the world.
    /// </summary>
    public sealed class WorldGrid
    {
        /// <summary>
        /// The size of one chunk in the world in pixels.
        /// </summary>
        public static int ChunkSize { get; set; } = 32;
        /// <summary>
        /// When generating chunks, how many chunks to generate around the position.
        /// </summary>
        public static int ChunkGenerationRadius { get; set; } = 4;

        /// <summary>
        /// The amount of chunks that were created.
        /// </summary>
        public int Count => chunks.Count;

        internal Dictionary<Vector2I, WorldChunk> AllChunks => chunks;

        private readonly Dictionary<Vector2I, WorldChunk> chunks = [];
        private World world;

        public WorldGrid(World world)
        {
            this.world = world;
        }

        /// <summary>
        /// gets the chunk 
        /// </summary>
        /// <param name="position">The chunk coordinate</param>
        /// <returns></returns>
        public WorldChunk GetChunkAt(Vector2I position)
        {
            // clamp the position to the nearest chunk
            position = new Vector2I((int)Math.Floor((float)position.X / ChunkSize) * ChunkSize, (int)Math.Floor((float)position.Y / ChunkSize) * ChunkSize);

            // check if the position is in the dictionary. if so, return the chunk at that position
            if (chunks.TryGetValue(position, out WorldChunk? value))
                return value;

            // the position is not directly a chunk position, so we need to check if it is in a chunk
            // find the chunk that contains the position

            foreach (WorldChunk chunk in chunks.Values)
            {
                if (chunk.Contains(position))
                    return chunk;
            }

            // The chunk does not exist.
            // Generate the chunk and return it
            GenerateChunks(position);

            // Call the method again to return the newly generated chunk
            return GetChunkAt(position);
        }

        public WorldChunk? GetChunkAt(int x, int y) => GetChunkAt(new Vector2I(x, y));

        public List<WorldObject> GetObjectsInChunk(Vector2I chunkPosition)
        {
            if (chunks.TryGetValue(chunkPosition, out WorldChunk? value))
            {
                List<WorldObject> objects = [];
                for (int i = 0; i < value.ObjectIndexes.Count; i++)
                {
                    int index = value.ObjectIndexes[i];
                    objects.Add(world[index]);
                }
                return objects.Where(x => x is not null).ToList();
            }
            else
                return [];
        }

        /// <summary>
        /// Gets all objects within the chunk at the specified chunk position, and 1 chunk away from it
        /// </summary>
        /// <param name="chunkPosition"></param>
        /// <returns></returns>
        public List<WorldObject> GetObjectsInAndAroundChunk(Vector2I chunkPosition)
        {
            List<WorldObject> objects = new List<WorldObject>();

            // clamp the chunk position to the nearest chunk
            chunkPosition = new Vector2I((int)Math.Floor((float)chunkPosition.X / ChunkSize) * ChunkSize, (int)Math.Floor((float)chunkPosition.Y / ChunkSize) * ChunkSize);

            for (int x = chunkPosition.X - ChunkSize; x <= chunkPosition.X + ChunkSize; x += ChunkSize)
            {
                for (int y = chunkPosition.Y - ChunkSize; y <= chunkPosition.Y + ChunkSize; y += ChunkSize)
                {
                    Vector2I chunkPos = new Vector2I(x, y);

                    // Check if chunk exists at the calculated position
                    var chunk = GetChunkAt(chunkPos);
                    if (chunk != null)
                    {
                        Debug.Log(chunk.ObjectIndexes.Count);
                        foreach (var obj in chunk.GetObjects())
                        {
                            if (!objects.Contains(obj))
                                objects.Add(obj);
                        }
                    }
                }
            }
            return objects;
        }

        /// <summary>
        /// Gets all objects around the given object. this respects the objects size and therefor allow for indexing more chunks than an 8x8 grid
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<WorldObject> GetObjectsAroundObject(WorldObject obj)
        {
            ObjectChunkData data = obj.ChunkPositionData;
            if(data == null)
                return [];

            // all chunks that the object contains
            List<WorldChunk> allChunks = [];

            List<WorldObject> objectsFound = [];

            // get all the objects within these, but also 1 chunk around the area of chunks the object is in
            foreach (WorldChunk chunk in data.Chunks)
            {
                allChunks.Add(chunk);
                foreach (WorldChunk neighbor in chunk.GetNeighboringChunks())
                {
                    if(!allChunks.Contains(neighbor))
                        allChunks.Add(neighbor);
                }
            }

            foreach(WorldChunk chunk in allChunks)
            {
                foreach(WorldObject chunkObj in chunk.GetObjects())
                {
                    if(!objectsFound.Contains(chunkObj))
                        objectsFound.Add(chunkObj);
                }
            }

            return objectsFound;
        }

        /// <summary>
        /// Generates chunks at the specified position. if there are already chunks there, nothing happens
        /// </summary>
        /// <param name="position"></param>
        public void GenerateChunks(Vector2I position)
        {
            GenerateChunks(position, ChunkGenerationRadius);
        }

        /// <summary>
        /// Generates chunks at the given position and the given radius around that position, if chunks already exist there, nothing happens
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        public void GenerateChunks(Vector2I position, int radius)
        {
            // clamp the position to the nearest chunk
            position = new Vector2I((int)Math.Floor((float)position.X / ChunkSize) * ChunkSize, (int)Math.Floor((float)position.Y / ChunkSize) * ChunkSize);

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2I chunkPosition = position + new Vector2I(x, y) * ChunkSize;
                    GenerateChunk(chunkPosition);
                }
            }
        }

        private void GenerateChunk(Vector2I position)
        {
            // Check if the chunk at this position already exists
            if (!chunks.ContainsKey(position))
            {
                // If not, create a new chunk and add it to the dictionary
                WorldChunk newChunk = new WorldChunk(position); // You need to implement this method
                chunks[position] = newChunk;
            }
        }

        internal void UpdateObjectChunkPositions(List<WorldObject> objects)
        {
            foreach (WorldChunk chunk in chunks.Values)
                chunk.ClearObjects();

            foreach (WorldObject obj in objects)
            {
                if (obj.index == -1)
                    obj.index = objects.IndexOf(obj);
                var objSize = GetObjectSize(obj);
                var chunksObjectIsIn = GetChunkArea(objSize.topLeft, objSize.bottomRight);

                obj.ChunkPositionData = new ObjectChunkData();
                var originChunk = GetChunkAt((Vector2I)obj.transform.position);
                obj.ChunkPositionData.ChunkContainingObjectOrigin = originChunk;

                foreach (var chunk in chunksObjectIsIn)
                {
                    // update all chunks that the object is in so that they know about the object, and all chunks that the object was in before, make them forget about the object
                    chunk.AddObject(obj.index);

                    // Make the object aware of what chunks it is in.
                    obj.ChunkPositionData.Chunks.Add(chunk);
                }
            }
        }

        private bool IsInvalidChunk(WorldChunk chunk)
        {
            // if the chunk is not aligned with the chunk grid, it is invalid.
            if (chunk.ChunkPosition.X % ChunkSize != 0 || chunk.ChunkPosition.Y % ChunkSize != 0)
                return true;
            return false;
        }

        internal WorldChunk[] GetChunkArea(Vector2I topLeft, Vector2I bottomRight)
        {
            if (topLeft == bottomRight)
                return [GetChunkAt(topLeft)];

            // round the values to the nearest chunk size
            topLeft = new Vector2I((int)Math.Floor((float)topLeft.X / ChunkSize) * ChunkSize, (int)Math.Floor((float)topLeft.Y / ChunkSize) * ChunkSize);
            bottomRight = new Vector2I((int)Math.Floor((float)bottomRight.X / ChunkSize) * ChunkSize, (int)Math.Floor((float)bottomRight.Y / ChunkSize) * ChunkSize);

            List<WorldChunk> chunks = new List<WorldChunk>();
            for (int x = topLeft.X; x <= bottomRight.X; x += ChunkSize)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y += ChunkSize)
                {
                    Vector2I chunkPosition = new Vector2I(x, y);
                    WorldChunk chunk = GetChunkAt(chunkPosition);
                    if (chunk is not null)
                        chunks.Add(chunk);
                    else
                    {
                        GenerateChunk(chunkPosition);
                        chunks.Add(GetChunkAt(chunkPosition));
                    }
                }
            }
            return chunks.ToArray();
        }

        /// <summary>
        /// Gets the size of the given object based on ceratin criteria, eg sprite renderer, collider
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public (Vector2I topLeft, Vector2I bottomRight) GetObjectSize(WorldObject obj)
        {
            if (obj is null)
                return (Vector2I.Zero, Vector2I.Zero);
            bool baseOnSprite = obj.TryFetchComponent<SpriteRenderer>(out SpriteRenderer? renderer);
            Collider collider = null;
            if (!obj.TryFetchComponent(out collider) && !baseOnSprite)
                return (Vector2I.Zero, Vector2I.Zero);
            if (collider is not null)
                baseOnSprite = false;

            if (baseOnSprite)
            {
                Vector2 size = renderer.Sprite.spriteSize;
                // take origin into account
                size.X *= renderer.Origin.X;
                size.Y *= renderer.Origin.Y;
                var result = (obj.transform.position - size / 2, obj.transform.position + size / 2);
                return ((Vector2I)result.Item1, (Vector2I)result.Item2);
            }
            else
            {
                Vector2 size = (Vector2I)collider!.Bounds.Size; // new(collider.Hitbox.Size.X, .Y);
                var result = (obj.transform.position - size / 2, obj.transform.position + size / 2);
                return ((Vector2I)result.Item1, (Vector2I)result.Item2);
            }
        }

        private Vector2I GetChunkPosition(Vector2 position)
        {
            return new Vector2I((int)Math.Floor(position.X / ChunkSize), (int)Math.Floor(position.Y / ChunkSize)) * ChunkSize;
        }

        /// <summary>
        /// Removes the given object from the chunkgrid entirely. making it unknown to all the chunks.
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Remove(WorldObject obj)
        {
            WorldChunk chunk = obj.ChunkPositionData.ChunkContainingObjectOrigin;
            chunk?.RemoveObject(obj.index);
        }

        /// <summary>
        /// Removes all chunks that are currently loaded. <br></br>
        /// This will not affect the world objects. <br></br>
        /// The frame after calling this method, any object that is not in a chunk will request new chunks to be generated.
        /// </summary>
        public void ResetChunks()
        {
            for (int i = 0; i < chunks.Count;)
            {
                WorldChunk chunk = chunks.ElementAt(i).Value;
                foreach (WorldObject obj in chunk.GetObjects())
                    if (obj != null)
                        obj.ChunkPositionData.ChunkContainingObjectOrigin = null!;
                if (!chunks.ContainsKey(chunk.ChunkPosition))
                    continue;
                chunks.Remove(chunk.ChunkPosition);
            }
        }
    }
}
