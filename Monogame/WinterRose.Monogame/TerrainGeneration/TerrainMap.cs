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

        private List<Chunk> chunks;
        private int seed;
        private float frequency;
        private float bias;
        private float colorNoiseIntensity;
        private TerrainGenerator terrainGenerator;

        public const int CHUNK_SIZE = 1000;
        public int Width { get; private set; }
        public int Height { get; private set; } 
        public int ChunkGenerationDistance { get; set; } = 2; 

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

            InitializeChunks(initialWidth, initialHeight, startingPosition);
        }

        private void InitializeChunks(int width, int height, Vector2 startingPosition)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
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
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            List<Chunk> newChunks = new List<Chunk>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int chunkX = x;
                    int chunkY = y;

                    Chunk? existingChunk = chunks.Find(c => c.position == new Vector2(chunkX, chunkY));
                    if (existingChunk != null)
                    {
                        newChunks.Add(existingChunk);
                    }
                    else
                    {
                        newChunks.Add(new Chunk(chunkX, chunkY, null!, this));
                    }
                }
            }

            chunks = newChunks;

            Width = width;
            Height = height;

            GenerateMap();
        }

        private void GenerateMap()
        {
            Parallel.For(0, chunks.Count,i =>
            {
                Chunk chunk = chunks[i];

                int offsetX = (int)(chunk.position.X * CHUNK_SIZE);
                int offsetY = (int)(chunk.position.Y * CHUNK_SIZE);

                chunk.Texture = terrainGenerator.CreateSubSprite(CHUNK_SIZE, CHUNK_SIZE, frequency, bias, colorNoiseIntensity, offsetX, offsetY, seed);
            });
        }


        public Sprite GetTexture(int chunkX, int chunkY)
        {
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
