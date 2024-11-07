using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.TerrainGeneration;

public class MapRenderer : Renderer
{
    public override RectangleF Bounds => new RectangleF(0, 0, 0, 0);

    public override TimeSpan DrawTime { get; protected set; }
    public TerrainMap Map { get; }

    public MapRenderer(TerrainMap map)
    {
        Map = map;
    }

    public override void Render(SpriteBatch batch)
    {
        List<TerrainMap.Chunk> list = Map.GetChunks();
        for (int i = 0; i < list.Count; i++)
        {
            TerrainMap.Chunk? chunk = list[i];
            if (chunk.Texture == null)
            {
                Map.RemoveChunk(chunk);
                continue;
            }
            Vector2 position = new Vector2((chunk.position.X * chunk.Texture.Width), (chunk.position.Y * chunk.Texture.Height)) + transform.position;

            batch.Draw(chunk.Texture, position, Color.White);

        }
    }
}

