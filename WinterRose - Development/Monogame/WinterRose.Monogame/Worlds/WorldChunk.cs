using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A chunk of the world containing data where what objects are in the world.
/// </summary>
[DebuggerDisplay("Objs: <{Count}>")]
public class WorldChunk(Vector2I position)
{
    /// <summary>
    /// The position of the chunk in the world (in world coordinates)
    /// </summary>
    public Vector2I ChunkPosition => position;

    /// <summary>
    /// A list of indexes of objects that find themselves in this chunk. The indexes are the indexes of the objects in the worlds int indexer
    /// </summary>
    public List<int> ObjectIndexes { get; set; } = [];

    /// <summary>
    /// Amount of objects in this chunk.
    /// </summary>
    public int Count => ObjectIndexes.Count;

    /// <summary>
    /// Checks if the given object is in this chunk
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool Contains(WorldObject obj) => Contains(obj.index);

    /// <summary>
    /// checks if the given index of an object is in this chunk
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool Contains(int index) => ObjectIndexes.Contains(index);

    /// <summary>
    /// Checks whether the given position is within the bounds of this chunk
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool Contains(Vector2I position)
    {
        // check if the position is within the bounds of the chunk
        if (position.X >= ChunkPosition.X * WorldGrid.ChunkSize && position.X < ChunkPosition.X * WorldGrid.ChunkSize + WorldGrid.ChunkSize)
            if (position.Y >= ChunkPosition.Y * WorldGrid.ChunkSize && position.Y < ChunkPosition.Y * WorldGrid.ChunkSize + WorldGrid.ChunkSize)
                return true;
        return false;
    }

    /// <summary>
    /// Adds the given object index to the chunk
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="Exception"></exception>
    internal void AddObject(int index)
    {
        ObjectIndexes.Add(index);
    }

    /// <summary>
    /// Removes the given object index from the chunk
    /// </summary>
    /// <param name="index"></param>
    internal void RemoveObject(int index)
    {
        ObjectIndexes.Remove(index);
    }

    /// <summary>
    /// Clears all objects from the chunk (does not affect the world or the objects themselves)
    /// </summary>
    internal void ClearObjects()
    {
        ObjectIndexes.Clear();
    }

    internal WorldObject[] GetObjects()
    {
        WorldObject[] objects = new WorldObject[ObjectIndexes.Count];
        for (int i = 0; i < ObjectIndexes.Count; i++)
            objects[i] = Universe.CurrentWorld[ObjectIndexes[i]];
        return objects;
    }

    /// <summary>
    /// Gets all objects within this chunk, and all 8 chunks around this one
    /// </summary>
    /// <returns></returns>
    public List<WorldObject> GetNearObjects()
    {
        // get all objects in the chunk and the surrounding chunks
        return Universe.CurrentWorld?.WorldChunkGrid.GetObjectsInAndAroundChunk(ChunkPosition) ?? [];
    }

    /// <summary>
    /// Gets
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public List<WorldObject> GetObjectsAroundObject(WorldObject obj) => Universe.CurrentWorld.WorldChunkGrid.GetObjectsAroundObject(obj);

    /// <summary>
    /// Gets all chunks around this one (including this one)
    /// </summary>
    /// <returns></returns>
    public List<WorldChunk> GetNeighboringChunks()
    {
        List<WorldChunk> chunks = [];
        int ChunkSize = WorldGrid.ChunkSize;
        for (int x = ChunkPosition.X - ChunkSize; x <= ChunkPosition.X + ChunkSize; x += ChunkSize)
        {
            for (int y = ChunkPosition.Y - ChunkSize; y <= ChunkPosition.Y + ChunkSize; y += ChunkSize)
            {
                Vector2I chunkPos = new Vector2I(x, y);

                chunks.Add(Universe.CurrentWorld.WorldChunkGrid.GetChunkAt(chunkPos));
            }
        }
        return chunks;
    }
}