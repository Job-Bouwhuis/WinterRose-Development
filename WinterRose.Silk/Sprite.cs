using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;
using Shader = WinterRose.SilkEngine.Shaders.ForgeShader;

namespace WinterRose.SilkEngine
{
    public class Sprite
    {
        public uint TextureID { get; private set; }
        public Vector2D<int> Size { get; private set; }

        public Sprite(string filePath, GL gl)
        {
            LoadTexture(filePath, gl);
        }

        private void LoadTexture(string filePath, GL gl)
        {
            // Load the image using StbImageSharp
            using FileStream file = File.OpenRead(filePath);
            ImageResult image = ImageResult.FromStream(file, ColorComponents.RedGreenBlueAlpha);

            if (image == null)
            {
                Console.WriteLine("Failed to load image.");
                return;
            }

            // Generate OpenGL texture
            TextureID = gl.GenTexture();
            gl.BindTexture(GLEnum.Texture2D, TextureID);

            // Use Span<byte> to create a reference type that can be passed as 'ref readonly'
            Span<byte> imageData = image.Data;
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)image.Width, (uint)image.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, ref imageData[0]);

            gl.GenerateMipmap(GLEnum.Texture2D);

            // Set texture parameters
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

            Size = new Vector2D<int>(image.Width, image.Height);
        }

        private Sprite()
        { }

        public static Sprite Square(int width, int height, Vector4D<byte> color, GL gl)
        {
            Sprite s = new Sprite();

            s.GenerateTexture(width, height, color, gl);
            return s;
        }

        private void GenerateTexture(int width, int height, Vector4D<byte> color, GL gl)
        {
            // Create a byte array for the texture data
            byte[] textureData = new byte[width * height * 4]; // RGBA

            // Fill the texture data array with the color
            for (int i = 0; i < width * height; i++)
            {
                textureData[i * 4] = color.X;   // Red
                textureData[i * 4 + 1] = color.Y; // Green
                textureData[i * 4 + 2] = color.Z; // Blue
                textureData[i * 4 + 3] = color.W; // Alpha
            }

            // Generate OpenGL texture
            TextureID = gl.GenTexture();
            gl.BindTexture(GLEnum.Texture2D, TextureID);

            Span<byte> imageData = textureData;

            // Upload the texture data to OpenGL
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, ref imageData[0]);

            gl.GenerateMipmap(GLEnum.Texture2D);

            // Set texture parameters
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

            Size = new Vector2D<int>(width, height);
        }

        public void Dispose(GL gl)
        {
            gl.DeleteTexture(TextureID);
        }
    }
}

