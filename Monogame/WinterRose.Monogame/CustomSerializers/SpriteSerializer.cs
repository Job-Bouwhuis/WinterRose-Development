using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame.CustomSerializers;
internal class SpriteSerializer : CustomSerializer<Sprite>
{
    public override object Deserialize(string data, int depth)
    {
        if(data.Contains("GeneratedTextureData"))
        {
            int sepIndex = data.IndexOf('|') + 2;
            data = data[sepIndex..(data.Length - 1)];
            var generatedData = SnowSerializerWorkers.DeserializeField<GeneratedTextureData>(data, 
                typeof(GeneratedTextureData), depth + 1, new(), new()) as GeneratedTextureData;
            return new Sprite(generatedData);
        }
        return new Sprite(data);
    }
    public override string Serialize(object obj, int depth)
    {
        Sprite sprite = obj as Sprite;
        if(sprite!.IsExternalTexture)
            return sprite.TexturePath!;

        GeneratedTextureData data = sprite.GeneratedTextureData;

        return SnowSerializerWorkers.SerializeField(data, "GeneratedTextureData", 
            typeof(GeneratedTextureData), depth+1, new(), new()).ToString();
    }
}

internal class GeneratedTextureDataSerializer : CustomSerializer<GeneratedTextureData>
{
    public override object Deserialize(string data, int depth)
    {
        if (data.Contains('@'))
            return SnowSerializerWorkers.DeserializeObject<GeneratedTextureData>(data, depth + 1, 
                typeof(GeneratedTextureData), new(), new());

        string[] content = data.Split('/');
        Color color = new Color(uint.Parse(content[0]));
        int width = int.Parse(content[1]);
        int height = int.Parse(content[2]);
        return GeneratedTextureData.Create(color, width, height);
    }
    public override string Serialize(object obj, int depth)
    {
        var data = obj as GeneratedTextureData;
        if (data.Pixels.All(x => x == data.Pixels[0]))
            return $"{data.Pixels[0].ToString()}/{data.Width}/{data.Height}";
        return SnowSerializerWorkers.SerializeObject(data, depth + 1, new(), new()).ToString();
    }
}
