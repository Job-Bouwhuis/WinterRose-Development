using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace WinterRose.Monogame.TerrainGeneration
{
    public class TerrainMap
    {
        public class Chunk
        {
            private Sprite texture;

            TerrainMap owner;

            public Vector2 position { get; }
            public Sprite Texture
            {
                get => texture;
                set => texture = value;
            }

            public Chunk(int chunkX, int chunkY, Sprite texture, TerrainMap owner)
            {
                position = new Vector2(chunkX, chunkY);
                Texture = texture;
                this.owner = owner;
            }
        }

        private List<Chunk> chunks; // List to store chunk information
        private int seed; // Seed for generating consistent noise
        private float frequency; // Frequency for Perlin noise
        private float bias; // Bias for noise values
        private float colorNoiseIntensity; // Intensity of color noise
        private TerrainGenerator terrainGenerator; // Instance of TerrainGenerator

        public const int CHUNK_SIZE = 1000;
        public int Width { get; private set; } // Width of the map in chunks
        public int Height { get; private set; } // Height of the map in chunks
        public int ChunkGenerationDistance { get; set; } = 2; // How many chunks away from the player to stop generating new chunks

        public TerrainMap(GraphicsDevice graphicsDevice, int initialWidth, int initialHeight, int seed, float frequency, float bias, float colorNoiseIntensity, Vector2 startingPosition)
        {
            this.seed = seed;
            if (seed == -1)
                this.seed = Random.Shared.Next(1, 100000000);
            this.frequency = frequency;
            this.bias = bias;
            this.colorNoiseIntensity = colorNoiseIntensity;
            terrainGenerator = new TerrainGenerator(graphicsDevice);
            chunks = new List<Chunk>();

            // Initialize the map with the specified dimensions
            InitializeChunks(initialWidth, initialHeight, startingPosition);
        }

        private void InitializeChunks(int width, int height, Vector2 startingPosition)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the actual position based on starting position
                    int chunkX = (int)startingPosition.X + x;
                    int chunkY = (int)startingPosition.Y + y;

                    chunks.Add(new Chunk(chunkX, chunkY, null, this));
                }
            }
            Width = width;
            Height = height;
            GenerateMap();
        }

        public void Resize(int width, int height)
        {
            // Calculate new half-width and half-height for centering
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            // Adjust chunks list by adding or removing chunks as needed
            List<Chunk> newChunks = new List<Chunk>();

            // Create new chunks based on the new dimensions
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int chunkX = x; // Only positive positions are considered
                    int chunkY = y;

                    // Check if the chunk already exists
                    Chunk? existingChunk = chunks.Find(c => c.position == new Vector2(chunkX, chunkY));
                    if (existingChunk != null)
                    {
                        // Retain the existing chunk if it exists
                        newChunks.Add(existingChunk);
                    }
                    else
                    {
                        // Create a new chunk if it doesn't exist
                        newChunks.Add(new Chunk(chunkX, chunkY, null!, this));
                    }
                }
            }

            // Update the chunks list with the new chunks
            chunks = newChunks;

            // Update Width and Height properties
            Width = width;
            Height = height;

            // Regenerate the map for the newly added chunks
            GenerateMap();
        }

        //private void GenerateMap()
        //{
        //    for (int i = 0; i < chunks.Count; i++)
        //    {
        //        Chunk? chunk = chunks[i];
        //        // Calculate offsets for Perlin noise generation
        //        int offsetX = (int)(chunk.position.X * CHUNK_SIZE);
        //        int offsetY = (int)(chunk.position.Y * CHUNK_SIZE);

        //        if(i % 100 == 0)
        //            Console.WriteLine($"{i} / {chunks.Count}");

        //        // Generate and assign the texture for the chunk
        //        chunk.Texture = terrainGenerator.CreateSubSprite(CHUNK_SIZE, CHUNK_SIZE, frequency, bias, colorNoiseIntensity, offsetX, offsetY, seed);
        //    }
        //}

        private void GenerateMap()
        {
            // Use Parallel.For to process chunks in parallel
            Parallel.For(0, chunks.Count,i =>
            {
                Chunk chunk = chunks[i];

                // Calculate offsets for Perlin noise generation
                int offsetX = (int)(chunk.position.X * CHUNK_SIZE);
                int offsetY = (int)(chunk.position.Y * CHUNK_SIZE);

                // Generate and assign the texture for the chunk
                chunk.Texture = terrainGenerator.CreateSubSprite(CHUNK_SIZE, CHUNK_SIZE, frequency, bias, colorNoiseIntensity, offsetX, offsetY, seed);
            });
        }


        public Sprite GetTexture(int chunkX, int chunkY)
        {
            // Check for the chunk in the list
            foreach (var chunk in chunks)
            {
                if (chunk.position == new Vector2(chunkX, chunkY))
                {
                    return chunk.Texture;
                }
            }
            throw new ArgumentOutOfRangeException("Requested texture is out of bounds.");
        }

        internal List<Chunk> GetChunks()
        {
            return chunks;
        }

        internal void RemoveChunk(Chunk chunk)
        {
            chunks.Remove(chunk);
        }

        public void SaveAll(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            foreach (Chunk chunk in chunks)
            {
                string fileName = chunk.position.ToString().TrimStart('{').TrimEnd('}').Replace(':', '-'); 
                if (chunk.Texture != null)
                    chunk.Texture.Save($"{dir}\\{fileName}.png");
            }

        }
    }
}
