using WinterRose.Serialization;
using System;
using System.IO;
using WinterRose.FileManagement;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WinterRose.Monogame;

/// <summary>
/// Represents an image that can be drawn to the screen
/// </summary>
[IncludePrivateFields]
public class Sprite
{
    /// <summary>
    /// The path of where the source of this sprite is. can be null if created using <see cref="MonoUtils.CreateTexture(int, int, byte[])"/> or other methods of creating a texture at runtime
    /// </summary>
    public string? TexturePath
    {
        get => texturePath;
        set
        {
            texturePath = value;
            if (value is null)
            {
                IsExternalTexture = false;
                GeneratedTextureData = new GeneratedTextureData(texture);
            }
            else
                IsExternalTexture = true;
        }
    }
    public object Tag => texture?.Tag;

    /// <summary>
    /// The name of the sprite
    /// </summary>
    public string? Name { get; set; }
    [ExcludeFromSerialization] private Texture2D? texture;
    private string? texturePath;

    internal bool IsExternalTexture { get; private set; } = false;
    internal GeneratedTextureData? GeneratedTextureData;
    internal Sprite(Texture2D texture, string name)
    {
        this.texture = texture;
        Name = name;
        texturePath = texture.Name;
        GeneratedTextureData = null;
    }
    internal Sprite(Texture2D sheet, Rectangle rect)
    {
        Color[] colors = new Color[rect.Width * rect.Height];
        sheet.GetData(0, rect, colors, 0, rect.Width * rect.Height);

        texture = new(MonoUtils.Graphics, rect.Width, rect.Height);
        texture.SetData(colors);
        TexturePath = texture.Name;
    }

    /// <summary>
    /// Creates a new empty sprite. This sprite has no data what so ever
    /// </summary>
    public Sprite()
    {
        texture = null;
        Name = null;
        texturePath = null;
        GeneratedTextureData = null;
    }
    /// <summary>
    /// Creates a new sprite object.<br></br><br></br>
    /// * file path: Used to load a texture, whether that be a .XNA file, or a .png/.jpg. when using this option, always exclude the file extention<br></br><br></br>
    /// * Base64 encoded byte array: this will createa texture at runtime with the provided bytes as color data
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="Exception">thrown when loading of the sprite failed</exception>
    public Sprite(string data)
    {
        try
        {
            texture = MonoUtils.Content.Load<Texture2D>(data);
            TexturePath = texture.Name;
            return;
        }
        catch { }
        try
        {
            GeneratedTextureData genData = SnowSerializer.Deserialize<GeneratedTextureData>(data).Result;
            if (genData != null)
            {
                GeneratedTextureData = genData;
                texture = GeneratedTextureData.MakeTexture();
                texturePath = null;
                IsExternalTexture = false;
            }
        }
        catch { }
        try
        {
            string toSeach;
            string targetName;
            string filePath = data.Replace('/', '\\');
            if (filePath.Contains('\\'))
            {
                toSeach = FileManager.PathOneUp(filePath);
                targetName = filePath.Split('\\', 
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
            }
            else
            {
                toSeach = "Content";
                targetName = data;
            }

            DirectoryInfo contentFolder = new DirectoryInfo(toSeach);
            FileInfo[] fileInfos = contentFolder.GetFiles();

            FileInfo? file = fileInfos.FirstOrDefault(x =>
            {
                return Path.GetFileNameWithoutExtension(x.Name) == targetName;
            });
            if(file is null)
            {
                file = fileInfos.FirstOrDefault(x =>
                {
                    return Path.GetFileName(x.Name) == targetName;
                });
            }

            if (file is not null && file.Exists)
            {
                FileStream stream = file.OpenRead();
                texture = Texture2D.FromStream(MonoUtils.Graphics, stream);
                TexturePath = FileManager.PathFrom(
                    Path.Combine(
                        FileManager.PathOneUp(file.FullName),
                        Path.GetFileNameWithoutExtension(file.FullName)),
                    "Content")[8..];
            }
            else
                throw new Exception("no file at data. attempting to load from Base64");
        }
        catch (InvalidOperationException e) when (e.Message is "This image format is not supported")
        {
            throw;
        }
        catch
        {
            try
            {
                LoadFromBase64(data);
            }
            catch
            {
                throw new Exception("Could not load texture from the data provided. \n" + data);
            }
        }
    }

    /// <summary>
    /// Creates a new sprite object, creating a new texture at runtime using <see cref="MonoUtils.CreateTexture(int, int, string)"/>
    /// </summary>
    public Sprite(int width, int height, string color)
    {
        texture = MonoUtils.CreateTexture(width, height, color);
        TexturePath = texture.Name;
    }
    /// <summary>
    /// Creates a new sprite object, creating a new texture at runtime using <see cref="MonoUtils.CreateTexture(int, int, Color, byte)"/>
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    public Sprite(int width, int height, Color color)
    {
        texture = MonoUtils.CreateTexture(width, height, color);
        TexturePath = texture.Name;
    }

    /// <summary>
    /// Creates a new sprite object, creating a new texture at runtime using <see cref="MonoUtils.CreateTexture(int, int, uint[])"/> It converts the <paramref name="colors"/> to a <see cref="uint"/> array
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="colors"></param>
    public Sprite(int width, int height, Color[] colors) : this(width, height, [.. colors.Select(x => x.PackedValue)]) { }

    /// <summary>
    /// Creates a new sprite object, creating a new texture at runtime using <see cref="MonoUtils.CreateTexture(int, int, uint[])"/>
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="colors"></param>
    public Sprite(int width, int height, uint[] colors)
    {
        texture = MonoUtils.CreateTexture(width, height, colors);
        TexturePath = texture.Name;
    }

    private void LoadFromBase64(string data)
    {
        data = data.Replace('♥', '=');
        var stuff = data.Split('^');
        int width = TypeWorker.CastPrimitive<int>(stuff[0]);
        int height = TypeWorker.CastPrimitive<int>(stuff[1]);

        var bytes = Convert.FromBase64String(stuff[2]);
        texture = MonoUtils.CreateTexture(width, height, bytes);
        TexturePath = texture.Name;
    }

    public static implicit operator string(Sprite s) => s.Name;
    public static implicit operator Texture2D(Sprite s)
    {
        try
        {
            return s.texture;
        }
        catch (NullReferenceException)
        {
            throw new Exception($"Sprite has no texture. > '{s.Name}'");
        }
    }
    public static implicit operator Sprite(Texture2D tex) => new(tex, "default") { TexturePath = tex.Name };

    /// <summary>
    /// Gets the width of this sprite
    /// </summary>
    public int Width
    {
        get => texture?.Width ?? 0;
    }
    /// <summary>
    /// Gets the height of this sprite
    /// </summary>
    public int Height
    {
        get => texture?.Height ?? 0;
    }
    /// <summary>
    /// Gets the dimention of this Sprite
    /// </summary>
    public Vector2I spriteSize
    {
        get => new(Width, Height);
    }
    /// <summary>
    /// Get the center of the sprite
    /// </summary>
    public Vector2 Center
    {
        get => new(Width / 2, Height / 2);
    }
    /// <summary>
    /// Returns the bounds of the texture, or a new <see cref="Rectangle"/> if there is no texture assigned to this <see cref="Sprite"/>
    /// </summary>
    public RectangleF Bounds
    {
        get
        {
            return texture?.Bounds ?? RectangleF.Zero;
        }
    }

    /// <summary>
    /// Gets the pixel data of this sprite
    /// </summary>
    /// <returns></returns>
    public Color[] GetPixelData()
    {
        if (texture == null)
            return Array.Empty<Color>();

        Color[] data = new Color[Width * Height];
        texture.GetData(data);
        return data;
    }
    /// <summary>
    /// does nothing atm
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public void SetPixelData(Color[] colors)
    {
        if (texture is null)
            throw new NullReferenceException();

        texture.SetData(colors);
    }
    /// <summary>
    /// Saves the sprite to a file, creates or overrides a file at <paramref name="path"/>
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void Save(string path)
    {
        if (texture is null)
            throw new NullReferenceException();
        texture.SaveAsPng(path);
    }
    /// <summary>
    /// Sets the texture that this sprite holds
    /// </summary>
    /// <param name="texture"></param>
    public void SetTexture(Texture2D texture)
    {
        this.texture = texture;
        TexturePath = texture.Name;
    }
    /// <summary>
    /// Sets the texture that the given <paramref name="sprite"/> holds.
    /// </summary>
    /// <param name="sprite"></param>
    public void SetTexture(Sprite sprite)
    {
        texture = sprite.texture;
        TexturePath = sprite.TexturePath;
    }

    /// <summary>
    /// Sets the pixel at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="color"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void SetPixel(int x, int y, Color color)
    {
        if (texture is null)
            throw new NullReferenceException();
        Color[] data = new Color[Width * Height];
        texture.GetData(data);
        data[x + y * Width] = color;
        texture.SetData(data);
    }

    /// <summary>
    /// Gets the pixel at (x, y)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Color GetPixel(int x, int y)
    {
        if (texture is null)
            throw new NullReferenceException();
        Color[] data = new Color[Width * Height];
        texture.GetData(data);
        return data[x + y * Width];
    }

    public static Sprite Circle(int radius, Color color)
    {
        // generate a circle texture with the given radius and color
        Texture2D tex = new Texture2D(MonoUtils.Graphics, radius * 2, radius * 2);

        // create a color array to hold the data
        Color[] data = new Color[radius * radius * 4];

        // calculate the diameter and radius
        float diam = radius / 2f;

        // the texture is a square so we need to loop through the entire texture and set the pixels that are not in the circle to transparent
        for (int x = 0; x < radius * 2; x++)
        {
            for (int y = 0; y < radius * 2; y++)
            {
                // calculate the distance from the center of the circle
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));

                // if the distance is greater than the radius, set the pixel to transparent
                if (dist > diam)
                    data[x + y * radius * 2] = Color.Transparent;
                else
                    data[x + y * radius * 2] = color;
            }
        }

        // set the data to the texture
        tex.SetData(data);

        return tex;
    }

    public static Sprite Noise(NoiseType noiseType, int width, int height, float frequency, float amplitude, float offsetX, float offsetY)
    {
       return NoiseGenerator.GenerateNoiseTexture(MonoUtils.Graphics, noiseType, width, height, frequency, amplitude, offsetX, offsetY);
    }
}
