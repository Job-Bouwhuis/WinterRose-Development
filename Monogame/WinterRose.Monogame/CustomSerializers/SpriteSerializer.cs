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
        return new Sprite(data);
    }
    public override string Serialize(object obj, int depth)
    {
        Sprite sprite = obj as Sprite;
        if(sprite!.IsExternalTexture)
            return sprite.TexturePath!;

        GeneratedTextureData data = sprite.GeneratedTextureData;

        return SnowSerializerWorkers.SerializeObject(data, depth, new() { CircleReferencesEnabled = false }, new()).ToString();
    }
}
