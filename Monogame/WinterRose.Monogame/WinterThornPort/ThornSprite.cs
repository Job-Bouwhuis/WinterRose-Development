using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.DefaultLibrary;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Monogame.WinterThornPort
{
    internal class ThornSprite : CSharpClass
    {
        Sprite sprite = new();

        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            Class result = new("Sprite", "A sprite");
            ThornSprite tex = this;
            result.CSharpClass = tex;

            result.DeclareFunction(new Function("CreateFromColor", "Creates a new sprite", AccessControl.Public)
            {
                CSharpFunction = (double with, double height, Class color) =>
                {
                    if(color.Name != "Color")
                    {
                        throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Sprite constructor that accepts a class can only accept the Color class");
                    }
                    else
                    {
                        ThornColor col = (ThornColor)color.CSharpClass;
                        tex.sprite = new Sprite((int)with, (int)height, col.color);
                    }
                }
            });
            result.DeclareFunction(new Function("CreateFromData", "Creates a new sprite", AccessControl.Public)
            {
                CSharpFunction = (double width, double height, string data) =>
                {
                    tex.sprite = new Sprite((int)width, (int)height, data);
                }
            });
            result.DeclareVariable(new Variable("width", "The width of the texture", AccessControl.Public)
            {
                Value = () => tex.sprite.Width,
                Setter = (object o) => { }
            });
            result.DeclareVariable(new Variable("height", "The height of the texture", AccessControl.Public)
            {
                Value = () => tex.sprite.Height,
                Setter = (object o) => { }
            });
            result.DeclareFunction(new Function("SetPixel", "Sets a pixel in the sprite", AccessControl.Public)
            {
                CSharpFunction = (double x, double y, Class color) =>
                {
                    if (color.Name != "Color")
                    {
                        throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Sprite.SetPixel can only accept the Color class");
                    }
                    else
                    {
                        ThornColor col = (ThornColor)color.CSharpClass;
                        tex.sprite.SetPixel((int)x, (int)y, col.color);
                    }
                }
            });
            result.DeclareFunction(new Function("GetPixel", "Gets a pixel in the sprite", AccessControl.Public)
            {
                CSharpFunction = (double x, double y) =>
                {
                    return new ThornColor() { color = tex.sprite.GetPixel((int)x, (int)y) };
                }
            });
            result.DeclareFunction(new Function("Save", "Saves the sprite to the disk at the given path", AccessControl.Public)
            {
                CSharpFunction = (string path) =>
                {
                    tex.sprite.Save(path);
                }
            });
            result.DeclareFunction(new Function("GetPixelData", "Gets all the pixels in a collection", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    var a = tex.sprite.GetPixelData()
                    .Select((x, i) => 
                        new Variable($"pixel{i}", $"the item at index {i}", new ThornColor() { color = x }, AccessControl.Private))
                    .ToArray();
                    return new Variable("PixelData", "", new Collection(a).GetClass(), AccessControl.Private);
                }
            });
            result.DeclareFunction(new Function("CreateFromCollection", "Creates a new sprite", AccessControl.Public)
            {
                CSharpFunction = (double width, double height, Class collection) =>
                {
                    if (collection.Name is not "Collection" or "ReadonlyCollection")
                        throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Sprite constructor that accepts a class can only accept the Collection class");
                    var col = (Collection)collection.CSharpClass;
                    var data = col.Values.Select(x => ((ThornColor)x.Value).color).ToArray();

                    tex.sprite = new Sprite((int)width, (int)height, data);
                }
            });

            return result;
        }
    }
}
